# How to Push This Project to GitHub ??

## Quick Start (3 Steps)

### Step 1: Create Repository on GitHub

1. Go to https://github.com
2. Click the **+** icon (top right) ? **New repository**
3. Fill in:
   - **Repository name:** `FastAPIHomeWifiQR`
   - **Description:** "WiFi QR Code API built with ASP.NET Core 8 & FastEndpoints"
   - **Visibility:** Public or Private
   - ? **DO NOT** check "Add a README" (we already have one)
   - ? **DO NOT** check "Add .gitignore" (we already have one)
4. Click **Create repository**
5. **Copy the repository URL** (e.g., `https://github.com/YOUR-USERNAME/FastAPIHomeWifiQR.git`)

---

### Step 2: Initialize Git & Commit

Open your terminal in the project folder and run:

```bash
# Navigate to your project
cd C:\Users\Akshitha\source\repos\FastAPIHomeWifiQR

# Initialize git repository
git init

# Add all files
git add .

# Create first commit
git commit -m "Initial commit: FastEndpoints WiFi QR API with JWT authentication"

# Rename branch to main (if needed)
git branch -M main
```

---

### Step 3: Push to GitHub

Replace `YOUR-USERNAME` with your actual GitHub username:

```bash
# Add remote repository
git remote add origin https://github.com/YOUR-USERNAME/FastAPIHomeWifiQR.git

# Push to GitHub
git push -u origin main
```

**Done!** ?? Your project is now on GitHub!

---

## Full Command List (Copy & Paste)

```bash
# Step 1: Initialize Git
cd C:\Users\Akshitha\source\repos\FastAPIHomeWifiQR
git init

# Step 2: Configure Git (if first time)
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"

# Step 3: Add files and commit
git add .
git commit -m "Initial commit: FastEndpoints WiFi QR API with JWT authentication"

# Step 4: Rename branch to main
git branch -M main

# Step 5: Add remote and push
git remote add origin https://github.com/YOUR-USERNAME/FastAPIHomeWifiQR.git
git push -u origin main
```

---

## ?? IMPORTANT: Security Before Pushing

### 1. Check appsettings.json

Make sure you don't commit sensitive data:

**appsettings.json** should look like this:
```json
{
  "JwtSettings": {
    "SigningKey": "CHANGE-THIS-IN-PRODUCTION",
    "Issuer": "HomeWifiQR",
    "Audience": "HomeWifiQRUsers",
    "ExpiryMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HomeWifiQRDb;..."
  }
}
```

? **Safe for development**  
?? **Replace secrets in production**

### 2. Verify .gitignore

Your `.gitignore` already excludes:
- ? Database files (*.db, *.sqlite)
- ? User secrets
- ? Environment files (.env)
- ? Build artifacts (bin/, obj/)

### 3. Check What's Being Committed

Before `git add .`, check:

```bash
git status
```

This shows what will be committed. Make sure no sensitive files are listed.

---

## Using Visual Studio

If you prefer using Visual Studio:

### Option 1: Visual Studio 2022

1. **Create Git Repository**
   - View ? Git Changes
   - Click **Create Git Repository**
   - Select **GitHub**
   - Enter repository name: `FastAPIHomeWifiQR`
   - Click **Create and Push**

### Option 2: Team Explorer

1. **Initialize Repository**
   - View ? Team Explorer
   - Click **Add to Source Control**
   - Select **Git**

2. **Commit Changes**
   - Enter commit message: "Initial commit"
   - Click **Commit All**

3. **Push to GitHub**
   - Click **Sync** ? **Publish Git Repo**
   - Select GitHub
   - Enter repository name
   - Click **Publish**

---

## Using VS Code

1. **Open Source Control**
   - Click Source Control icon (left sidebar)
   - Or press `Ctrl + Shift + G`

2. **Initialize Repository**
   - Click **Initialize Repository**

3. **Stage All Changes**
   - Click **+** next to "Changes"

4. **Commit**
   - Enter message: "Initial commit"
   - Click ? (checkmark) or press `Ctrl + Enter`

5. **Publish to GitHub**
   - Click **Publish to GitHub**
   - Select **Public** or **Private**
   - Select files to include
   - Click **OK**

---

## Verify Your Push

After pushing, verify:

1. Visit: `https://github.com/YOUR-USERNAME/FastAPIHomeWifiQR`
2. You should see:
   - ? All your code files
   - ? README.md displayed on homepage
   - ? .gitignore file
   - ? Documentation files

---

## Common Issues & Solutions

### Issue 1: "Permission denied (publickey)"

**Solution:** Use HTTPS instead of SSH

```bash
# Remove existing remote
git remote remove origin

# Add HTTPS remote
git remote add origin https://github.com/YOUR-USERNAME/FastAPIHomeWifiQR.git

# Push
git push -u origin main
```

### Issue 2: "Authentication failed"

**Solution:** Use GitHub Personal Access Token

1. Go to GitHub Settings ? Developer settings ? Personal access tokens ? Tokens (classic)
2. Generate new token with `repo` scope
3. Use token as password when prompted

### Issue 3: "Updates were rejected"

**Solution:** Force push (only if you're sure)

```bash
git push -u origin main --force
```

### Issue 4: "fatal: not a git repository"

**Solution:** You're not in the project folder

```bash
cd C:\Users\Akshitha\source\repos\FastAPIHomeWifiQR
git init
```

---

## Next Steps After Pushing

### 1. Add GitHub Actions (CI/CD)

Create `.github/workflows/dotnet.yml`:

```yaml
name: .NET

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

### 2. Add Badges to README

```markdown
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![FastEndpoints](https://img.shields.io/badge/FastEndpoints-5.30-blue)
![License](https://img.shields.io/badge/license-MIT-green)
```

### 3. Protect Secrets

For production deployment:

```bash
# Use environment variables
export JwtSettings__SigningKey="your-production-key"

# Or use .NET User Secrets (development)
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SigningKey" "your-secret-key"
```

---

## Update README with Your Info

Before pushing, update `README.md`:

1. Replace `YOUR-USERNAME` with your GitHub username
2. Add your name in the Author section
3. Update the repository URL
4. Add your Twitter/LinkedIn (optional)

---

## Git Commands Cheat Sheet

```bash
# Check status
git status

# Add specific file
git add filename.cs

# Add all files
git add .

# Commit with message
git commit -m "Your message"

# Push to GitHub
git push

# Pull latest changes
git pull

# View commit history
git log

# Create new branch
git checkout -b feature/new-feature

# Switch branch
git checkout main

# View remote
git remote -v
```

---

## ?? You're Done!

Your project is now on GitHub! Share it:

```
https://github.com/YOUR-USERNAME/FastAPIHomeWifiQR
```

Remember to:
- ? Star your own repo (for visibility)
- ?? Add topics/tags (ASP.NET Core, FastEndpoints, JWT, etc.)
- ?? Add a LICENSE file
- ?? Never commit secrets or API keys

---

Happy coding! ??
