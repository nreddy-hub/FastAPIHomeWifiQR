# Quick Start - React WiFi QR Generator

## ?? Create Project in 5 Minutes

Copy and paste these commands:

```bash
# Step 1: Create React app
cd C:\Users\Akshitha\source\repos
npm create vite@latest wifi-qr-generator -- --template react-ts
cd wifi-qr-generator

# Step 2: Install all dependencies
npm install axios react-router-dom react-hook-form react-hot-toast qrcode.react @types/react-router-dom

# Step 3: Start development server
npm run dev
```

That's it! Now copy the code from `REACT_SETUP_GUIDE.md` into the files.

---

## ?? Files to Create

After running the commands above, create these files:

### 1. `.env`
```env
VITE_API_BASE_URL=http://localhost:5014/api
```

### 2. `src/types/wifi.types.ts`
? See REACT_SETUP_GUIDE.md Step 5

### 3. `src/services/api.ts`
? See REACT_SETUP_GUIDE.md Step 6

### 4. `src/components/WifiForm.tsx`
? See REACT_SETUP_GUIDE.md Step 7.1

### 5. `src/components/WifiList.tsx`
? See REACT_SETUP_GUIDE.md Step 7.2

### 6. `src/components/Login.tsx`
? See REACT_SETUP_GUIDE.md Step 7.3

### 7. `src/App.tsx` (replace existing)
? See REACT_SETUP_GUIDE.md Step 7.4

### 8. `src/App.css` (replace existing)
? See REACT_SETUP_GUIDE.md Step 7.5

---

## ? Final Checklist

- [ ] API is running: http://localhost:5014
- [ ] CORS enabled in `Program.cs`
- [ ] React app created with Vite
- [ ] All npm packages installed
- [ ] All files created from guide
- [ ] `.env` file created
- [ ] React app running: http://localhost:5173

---

## ?? Test Flow

1. Open http://localhost:5173
2. Login with: admin / password123
3. Create a WiFi network
4. See QR code generated instantly
5. Download the QR code
6. Go to "My Networks" to see all
7. Select multiple and download as ZIP

---

## ?? What You Get

? User-friendly WiFi QR code generator  
? Login/Logout with JWT  
? Create WiFi networks  
? Instant QR code generation  
? Download individual QR codes  
? Bulk download as ZIP  
? View all networks  
? Fully responsive design  

---

Need help? Check `REACT_SETUP_GUIDE.md` for detailed instructions!
