# React WiFi QR Generator - Setup Guide ??

This guide will help you create a React app that lets users generate WiFi QR codes using your FastEndpoints API.

---

## ?? Project Overview

**What it does:**
- New users can enter WiFi details (SSID, Password, Encryption)
- Submit the form to generate a WiFi QR code
- Download the QR code image
- View all created WiFi networks
- Bulk download multiple QR codes

---

## ??? Step 1: Create React Project

Open a **new terminal** and run:

```bash
# Navigate to your repos folder
cd C:\Users\Akshitha\source\repos

# Create React app with Vite (Recommended - Fast!)
npm create vite@latest wifi-qr-generator -- --template react-ts

# Navigate to the project
cd wifi-qr-generator

# Install dependencies
npm install
```

**Alternative: Create React App**
```bash
npx create-react-app wifi-qr-generator --template typescript
cd wifi-qr-generator
```

---

## ?? Step 2: Install Required Packages

```bash
# API calls
npm install axios

# Routing
npm install react-router-dom

# UI Library (Choose ONE)
npm install @mui/material @emotion/react @emotion/styled @mui/icons-material

# OR use Ant Design
# npm install antd

# Form handling
npm install react-hook-form

# Toast notifications
npm install react-hot-toast

# QR Code display (optional - for preview)
npm install qrcode.react

# TypeScript types
npm install --save-dev @types/react-router-dom
```

---

## ?? Step 3: Project Structure

Create this folder structure:

```
wifi-qr-generator/
??? src/
?   ??? components/
?   ?   ??? WifiForm.tsx          # Main form for WiFi details
?   ?   ??? WifiList.tsx           # List of all WiFi networks
?   ?   ??? QRCodeDisplay.tsx      # Display and download QR
?   ?   ??? Login.tsx              # Login form
?   ?   ??? Navbar.tsx             # Navigation bar
?   ??? services/
?   ?   ??? api.ts                 # API service layer
?   ??? types/
?   ?   ??? wifi.types.ts          # TypeScript types
?   ??? contexts/
?   ?   ??? AuthContext.tsx        # Authentication context
?   ??? App.tsx
?   ??? main.tsx
?   ??? App.css
??? .env                           # Environment variables
??? package.json
```

---

## ?? Step 4: Configuration Files

### Create `.env` file in project root:

```env
VITE_API_BASE_URL=http://localhost:5014/api
```

### Update `vite.config.ts`:

```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5014',
        changeOrigin: true,
      }
    }
  }
})
```

---

## ?? Step 5: Create Type Definitions

**Create `src/types/wifi.types.ts`:**

```typescript
export interface WifiNetwork {
  id: string;
  ssid: string;
  password: string | null;
  encryption: string;
  hidden: boolean;
}

export interface CreateWifiRequest {
  ssid: string;
  password?: string;
  encryption: string;
  hidden: boolean;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  token?: string;
  expiresAt?: string;
}

export type EncryptionType = 'WPA' | 'WPA2' | 'WPA3' | 'WEP' | 'nopass';
```

---

## ?? Step 6: Create API Service

**Create `src/services/api.ts`:**

```typescript
import axios from 'axios';
import { CreateWifiRequest, LoginRequest, WifiNetwork } from '../types/wifi.types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5014/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add JWT token to all requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('jwt_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle errors globally
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('jwt_token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authService = {
  login: async (username: string, password: string) => {
    const response = await api.post<LoginRequest>('/auth/login', { username, password });
    return response.data;
  },
  
  logout: () => {
    localStorage.removeItem('jwt_token');
  },
  
  isAuthenticated: () => {
    return !!localStorage.getItem('jwt_token');
  },
};

export const wifiService = {
  // Get all WiFi networks
  getAll: async (): Promise<WifiNetwork[]> => {
    const response = await api.get('/wifi');
    return response.data;
  },

  // Get single WiFi network
  getById: async (id: string): Promise<WifiNetwork> => {
    const response = await api.get(`/wifi/${id}`);
    return response.data;
  },

  // Create new WiFi network
  create: async (data: CreateWifiRequest): Promise<WifiNetwork> => {
    const response = await api.post('/wifi', data);
    return response.data;
  },

  // Download QR code as blob
  downloadQR: async (id: string): Promise<Blob> => {
    const response = await api.get(`/wifi/${id}/qr`, {
      responseType: 'blob',
    });
    return response.data;
  },

  // Download multiple QR codes as ZIP
  downloadBulkQR: async (ids: string[]): Promise<Blob> => {
    const response = await api.post('/wifi/bulk-qr', { ids }, {
      responseType: 'blob',
    });
    return response.data;
  },
};

export default api;
```

---

## ?? Step 7: Create Components

### **1. WiFi Form Component**

**Create `src/components/WifiForm.tsx`:**

```typescript
import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { wifiService } from '../services/api';
import { CreateWifiRequest, EncryptionType } from '../types/wifi.types';

interface FormData {
  ssid: string;
  password: string;
  encryption: EncryptionType;
  hidden: boolean;
}

const WifiForm: React.FC<{ onSuccess?: () => void }> = ({ onSuccess }) => {
  const [qrImageUrl, setQrImageUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  
  const { register, handleSubmit, watch, formState: { errors }, reset } = useForm<FormData>({
    defaultValues: {
      encryption: 'WPA2',
      hidden: false,
    }
  });

  const encryption = watch('encryption');

  const onSubmit = async (data: FormData) => {
    setLoading(true);
    try {
      // Create WiFi network
      const request: CreateWifiRequest = {
        ssid: data.ssid,
        password: encryption === 'nopass' ? undefined : data.password,
        encryption: data.encryption,
        hidden: data.hidden,
      };

      const result = await wifiService.create(request);
      
      // Download QR code
      const qrBlob = await wifiService.downloadQR(result.id);
      const imageUrl = URL.createObjectURL(qrBlob);
      setQrImageUrl(imageUrl);

      toast.success('WiFi QR code generated successfully!');
      reset();
      onSuccess?.();
    } catch (error: any) {
      const errorMsg = error.response?.data?.errors 
        ? Object.values(error.response.data.errors).flat().join(', ')
        : 'Failed to generate QR code';
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  const downloadQR = () => {
    if (qrImageUrl) {
      const link = document.createElement('a');
      link.href = qrImageUrl;
      link.download = 'wifi-qrcode.png';
      link.click();
    }
  };

  return (
    <div style={{ maxWidth: '600px', margin: '0 auto', padding: '20px' }}>
      <h2>Generate WiFi QR Code</h2>
      
      <form onSubmit={handleSubmit(onSubmit)} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        {/* SSID */}
        <div>
          <label>WiFi Name (SSID) *</label>
          <input
            {...register('ssid', { 
              required: 'SSID is required',
              maxLength: { value: 32, message: 'SSID cannot exceed 32 characters' }
            })}
            type="text"
            placeholder="My WiFi Network"
            style={{ width: '100%', padding: '8px' }}
          />
          {errors.ssid && <span style={{ color: 'red' }}>{errors.ssid.message}</span>}
        </div>

        {/* Encryption */}
        <div>
          <label>Encryption Type *</label>
          <select {...register('encryption')} style={{ width: '100%', padding: '8px' }}>
            <option value="WPA">WPA</option>
            <option value="WPA2">WPA2</option>
            <option value="WPA3">WPA3</option>
            <option value="WEP">WEP</option>
            <option value="nopass">No Password (Open)</option>
          </select>
        </div>

        {/* Password (only for encrypted networks) */}
        {encryption !== 'nopass' && (
          <div>
            <label>Password *</label>
            <input
              {...register('password', {
                required: encryption !== 'nopass' ? 'Password is required for encrypted networks' : false,
                minLength: { value: 8, message: 'Password must be at least 8 characters' },
                maxLength: { value: 63, message: 'Password cannot exceed 63 characters' }
              })}
              type="password"
              placeholder="Your WiFi password"
              style={{ width: '100%', padding: '8px' }}
            />
            {errors.password && <span style={{ color: 'red' }}>{errors.password.message}</span>}
          </div>
        )}

        {/* Hidden */}
        <div>
          <label>
            <input type="checkbox" {...register('hidden')} />
            {' '}Hidden Network
          </label>
        </div>

        {/* Submit Button */}
        <button 
          type="submit" 
          disabled={loading}
          style={{ 
            padding: '10px 20px', 
            backgroundColor: '#1976d2', 
            color: 'white', 
            border: 'none',
            borderRadius: '4px',
            cursor: loading ? 'not-allowed' : 'pointer'
          }}
        >
          {loading ? 'Generating...' : 'Generate QR Code'}
        </button>
      </form>

      {/* Display QR Code */}
      {qrImageUrl && (
        <div style={{ marginTop: '30px', textAlign: 'center' }}>
          <h3>Your WiFi QR Code</h3>
          <img src={qrImageUrl} alt="WiFi QR Code" style={{ maxWidth: '300px', border: '1px solid #ddd', padding: '10px' }} />
          <br />
          <button 
            onClick={downloadQR}
            style={{ 
              marginTop: '10px',
              padding: '10px 20px',
              backgroundColor: '#4caf50',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer'
            }}
          >
            Download QR Code
          </button>
        </div>
      )}
    </div>
  );
};

export default WifiForm;
```

---

### **2. WiFi List Component**

**Create `src/components/WifiList.tsx`:**

```typescript
import React, { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { wifiService } from '../services/api';
import { WifiNetwork } from '../types/wifi.types';

const WifiList: React.FC = () => {
  const [networks, setNetworks] = useState<WifiNetwork[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  useEffect(() => {
    loadNetworks();
  }, []);

  const loadNetworks = async () => {
    try {
      const data = await wifiService.getAll();
      setNetworks(data);
    } catch (error) {
      toast.error('Failed to load WiFi networks');
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadQR = async (id: string, ssid: string) => {
    try {
      const blob = await wifiService.downloadQR(id);
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${ssid}-qrcode.png`;
      link.click();
      toast.success('QR code downloaded!');
    } catch (error) {
      toast.error('Failed to download QR code');
    }
  };

  const handleBulkDownload = async () => {
    if (selectedIds.length === 0) {
      toast.error('Please select at least one network');
      return;
    }

    try {
      const blob = await wifiService.downloadBulkQR(selectedIds);
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'wifi-qrcodes.zip';
      link.click();
      toast.success(`Downloaded ${selectedIds.length} QR codes!`);
      setSelectedIds([]);
    } catch (error) {
      toast.error('Failed to download QR codes');
    }
  };

  const toggleSelection = (id: string) => {
    setSelectedIds(prev => 
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  if (loading) return <div>Loading...</div>;

  return (
    <div style={{ maxWidth: '800px', margin: '0 auto', padding: '20px' }}>
      <h2>WiFi Networks</h2>
      
      {selectedIds.length > 0 && (
        <button 
          onClick={handleBulkDownload}
          style={{ 
            marginBottom: '20px',
            padding: '10px 20px',
            backgroundColor: '#ff9800',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Download {selectedIds.length} QR Code{selectedIds.length > 1 ? 's' : ''} as ZIP
        </button>
      )}

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ backgroundColor: '#f5f5f5' }}>
            <th style={{ padding: '10px', textAlign: 'left' }}>
              <input 
                type="checkbox"
                onChange={(e) => setSelectedIds(e.target.checked ? networks.map(n => n.id) : [])}
                checked={selectedIds.length === networks.length && networks.length > 0}
              />
            </th>
            <th style={{ padding: '10px', textAlign: 'left' }}>SSID</th>
            <th style={{ padding: '10px', textAlign: 'left' }}>Encryption</th>
            <th style={{ padding: '10px', textAlign: 'left' }}>Hidden</th>
            <th style={{ padding: '10px', textAlign: 'left' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {networks.map((network) => (
            <tr key={network.id} style={{ borderBottom: '1px solid #ddd' }}>
              <td style={{ padding: '10px' }}>
                <input 
                  type="checkbox"
                  checked={selectedIds.includes(network.id)}
                  onChange={() => toggleSelection(network.id)}
                />
              </td>
              <td style={{ padding: '10px' }}>{network.ssid}</td>
              <td style={{ padding: '10px' }}>{network.encryption}</td>
              <td style={{ padding: '10px' }}>{network.hidden ? 'Yes' : 'No'}</td>
              <td style={{ padding: '10px' }}>
                <button 
                  onClick={() => handleDownloadQR(network.id, network.ssid)}
                  style={{ 
                    padding: '5px 10px',
                    backgroundColor: '#4caf50',
                    color: 'white',
                    border: 'none',
                    borderRadius: '4px',
                    cursor: 'pointer'
                  }}
                >
                  Download QR
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {networks.length === 0 && (
        <p style={{ textAlign: 'center', marginTop: '20px', color: '#666' }}>
          No WiFi networks found. Create one to get started!
        </p>
      )}
    </div>
  );
};

export default WifiList;
```

---

### **3. Login Component**

**Create `src/components/Login.tsx`:**

```typescript
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { authService } from '../services/api';

const Login: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      const response = await authService.login(username, password);
      
      if (response.success && response.token) {
        localStorage.setItem('jwt_token', response.token);
        toast.success('Login successful!');
        navigate('/');
      } else {
        toast.error(response.message || 'Login failed');
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Invalid username or password');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ 
      maxWidth: '400px', 
      margin: '100px auto', 
      padding: '30px',
      border: '1px solid #ddd',
      borderRadius: '8px',
      boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
    }}>
      <h2 style={{ textAlign: 'center' }}>Login</h2>
      
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        <div>
          <label>Username</label>
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            style={{ width: '100%', padding: '8px' }}
          />
        </div>

        <div>
          <label>Password</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            style={{ width: '100%', padding: '8px' }}
          />
        </div>

        <button 
          type="submit"
          disabled={loading}
          style={{ 
            padding: '10px',
            backgroundColor: '#1976d2',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: loading ? 'not-allowed' : 'pointer'
          }}
        >
          {loading ? 'Logging in...' : 'Login'}
        </button>
      </form>

      <p style={{ marginTop: '20px', textAlign: 'center', fontSize: '14px', color: '#666' }}>
        Default: admin / password123
      </p>
    </div>
  );
};

export default Login;
```

---

### **4. App Component**

**Update `src/App.tsx`:**

```typescript
import React from 'react';
import { BrowserRouter, Routes, Route, Navigate, Link } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import WifiForm from './components/WifiForm';
import WifiList from './components/WifiList';
import Login from './components/Login';
import { authService } from './services/api';
import './App.css';

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return authService.isAuthenticated() ? <>{children}</> : <Navigate to="/login" />;
};

const Navbar: React.FC = () => {
  const isAuth = authService.isAuthenticated();

  const handleLogout = () => {
    authService.logout();
    window.location.href = '/login';
  };

  return (
    <nav style={{ 
      padding: '15px 30px', 
      backgroundColor: '#1976d2', 
      color: 'white',
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center'
    }}>
      <h1 style={{ margin: 0 }}>WiFi QR Generator</h1>
      
      {isAuth && (
        <div style={{ display: 'flex', gap: '20px', alignItems: 'center' }}>
          <Link to="/" style={{ color: 'white', textDecoration: 'none' }}>Create QR</Link>
          <Link to="/list" style={{ color: 'white', textDecoration: 'none' }}>My Networks</Link>
          <button 
            onClick={handleLogout}
            style={{ 
              padding: '8px 16px',
              backgroundColor: 'rgba(255,255,255,0.2)',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer'
            }}
          >
            Logout
          </button>
        </div>
      )}
    </nav>
  );
};

function App() {
  return (
    <BrowserRouter>
      <div className="App">
        <Navbar />
        <Toaster position="top-right" />
        
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/" element={
            <ProtectedRoute>
              <WifiForm />
            </ProtectedRoute>
          } />
          <Route path="/list" element={
            <ProtectedRoute>
              <WifiList />
            </ProtectedRoute>
          } />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

export default App;
```

---

### **5. Update `src/App.css`:**

```css
* {
  box-sizing: border-box;
}

body {
  margin: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
    sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

.App {
  min-height: 100vh;
  background-color: #f5f5f5;
}

input, select, textarea {
  font-family: inherit;
  font-size: 14px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

input:focus, select:focus, textarea:focus {
  outline: none;
  border-color: #1976d2;
}

button {
  font-family: inherit;
  font-size: 14px;
}

button:hover:not(:disabled) {
  opacity: 0.9;
}

table {
  background: white;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}
```

---

## ?? Step 8: Run the Projects

### Terminal 1: Run API
```bash
cd C:\Users\Akshitha\source\repos\FastAPIHomeWifiQR
dotnet run
```

### Terminal 2: Run React App
```bash
cd C:\Users\Akshitha\source\repos\wifi-qr-generator
npm run dev
```

---

## ?? Access the Applications

- **API (Swagger):** http://localhost:5014/swagger
- **React App:** http://localhost:5173

---

## ? Test the Flow

1. **Login:**
   - Username: `admin`
   - Password: `password123`

2. **Create WiFi QR:**
   - Enter SSID, Password, Encryption
   - Click "Generate QR Code"
   - QR code appears instantly
   - Download the image

3. **View All Networks:**
   - Click "My Networks"
   - See all created WiFi networks
   - Download individual QR codes
   - Select multiple and download as ZIP

---

## ?? Customization Ideas

1. **Add Material-UI for better styling**
2. **Add QR code preview before creation**
3. **Add edit/delete functionality**
4. **Add password strength indicator**
5. **Add print functionality for QR codes**
6. **Add dark mode**
7. **Add WiFi network categories/tags**

---

## ?? Deployment

### Deploy API:
- Azure App Service
- AWS Elastic Beanstalk
- Docker container

### Deploy React:
- **Vercel** (Recommended): `npm run build` ? Deploy
- **Netlify**: Automatic deployment from Git
- **Azure Static Web Apps**: Free tier available

---

## ?? Next Steps

1. Follow this guide to create the React project
2. Copy all the code into respective files
3. Run both projects
4. Test the complete flow
5. Customize as needed

---

Your React app is ready! ??

Let me know if you need help with any specific component or feature!
