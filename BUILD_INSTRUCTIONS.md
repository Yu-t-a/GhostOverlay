# คู่มือการสร้าง .exe และ .msi สำหรับ GhostOverlay

## วิธีที่ 1: ใช้ Build Script อัตโนมัติ (แนะนำ - ง่ายที่สุด!)

```powershell
.\build-installer.ps1
```

สคริปต์นี้จะทำทุกอย่างให้คุณ:
- ✅ ล้าง build เก่า
- ✅ สร้าง .exe แบบ self-contained (ไม่ต้องติดตั้ง .NET)
- ✅ ตรวจสอบและติดตั้ง WiX Toolset
- ✅ สร้าง .msi installer

**ผลลัพธ์:**
```
publish\win-x64\GhostOverlay.exe          ← ไฟล์ .exe พร้อมใช้งาน
installer\bin\Release\GhostOverlaySetup.msi  ← ไฟล์ installer .msi
```

---

## วิธีที่ 2: สร้าง .exe เอง (ทำทีละขั้นตอน)

### ขั้นตอนที่ 1: Build .exe แบบ Self-Contained

```powershell
# ล้าง build เก่า
dotnet clean

# สร้าง .exe (ไฟล์เดียว รวม .NET Runtime ไม่ต้องติดตั้ง .NET)
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64
```

**คำอธิบาย:**
- `-c Release` = Build แบบ optimized
- `-r win-x64` = สำหรับ Windows 64-bit
- `--self-contained` = รวม .NET Runtime (ไม่ต้องติดตั้ง .NET แยก)
- `-p:PublishSingleFile=true` = สร้างเป็นไฟล์เดียว
- `-o publish/win-x64` = บันทึกไฟล์ที่โฟลเดอร์ publish

**ผลลัพธ์:**
```
publish\win-x64\GhostOverlay.exe
ขนาด: ~150-200 MB
ใช้งานได้ทันที: ✅ (ไม่ต้องติดตั้ง .NET)
```

### ขั้นตอนที่ 2: แจกจ่ายไฟล์ .exe

**วิธีที่ 1 - แจกเป็นไฟล์ .exe เดี่ยว:**
```powershell
# คัดลอกไฟล์ไปที่ต้องการ
copy publish\win-x64\GhostOverlay.exe C:\MyApps\
```

**วิธีที่ 2 - แจกเป็นไฟล์ ZIP:**
```powershell
# สร้างไฟล์ ZIP สำหรับแจกจ่าย
Compress-Archive -Path "publish\win-x64\*" -DestinationPath "GhostOverlay-v1.0.0-Portable.zip" -Force
```

---

## วิธีที่ 3: สร้าง .msi Installer (Professional Installer)

### ขั้นตอนที่ 1: ติดตั้ง WiX Toolset v5

```powershell
# ติดตั้ง WiX ผ่าน .NET Tool
dotnet tool install --global wix
```

**หรือดาวน์โหลดจากเว็บ:**
- ไปที่: https://wixtoolset.org/releases/
- ดาวน์โหลดและติดตั้ง WiX v5.x

### ขั้นตอนที่ 2: สร้างไฟล์ Icon (ถ้ายังไม่มี)

```powershell
# ถ้ายังไม่มีไฟล์ icon.ico ให้สร้างไฟล์ว่างหรือหาไอคอนมาใส่
# หรือลบการอ้างอิง icon ออกจาก Product.wxs ก่อน
```

### ขั้นตอนที่ 3: Build .msi

```powershell
# เข้าไปในโฟลเดอร์ installer
cd installer

# Build .msi installer
dotnet build -c Release

# กลับมาที่โฟลเดอร์รากโปรเจค
cd ..
```

**ผลลัพธ์:**
```
installer\bin\Release\net8.0-windows\GhostOverlaySetup.msi
```

### ความสามารถของ .msi Installer:
- ✅ ติดตั้งแบบมาตรฐาน Windows
- ✅ สร้าง shortcuts ใน Start Menu
- ✅ สร้าง shortcuts บน Desktop
- ✅ ติดตั้งที่ Program Files
- ✅ แสดงใน Add/Remove Programs
- ✅ ถอนการติดตั้งได้สะดวก

---

## Build Commands Reference

### Rebuild Everything:
```bash
# Clean
dotnet clean

# Restore packages
dotnet restore

# Build Release (self-contained, single file)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

### Build for Multiple Platforms:
```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Windows x86
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true

# Windows ARM64
dotnet publish -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true
```

### Optimize File Size:
```bash
# With trimming (reduces size but may cause issues)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

# Ready-to-run (faster startup, larger file)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true
```

---

## Troubleshooting

### Issue: .exe is too large (155 MB)
**Solution 1:** Use framework-dependent deployment (requires .NET installed on user's machine)
```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
# Result: ~5 MB exe, but requires .NET 8.0 Runtime
```

**Solution 2:** Enable trimming (may cause runtime issues)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
# Result: ~60-80 MB
```

### Issue: Missing app.ico
The installer script expects an icon at `Resources\app.ico`. If missing:
- Option 1: Create an icon file and place it there
- Option 2: Remove `SetupIconFile` line from `.iss` file

### Issue: Inno Setup not installed
Download from: https://jrsoftware.org/isdl.php (free, ~2 MB)

---

## Distribution Checklist

Before releasing:

1. **Test the .exe**
   - [ ] Run on clean Windows machine without .NET installed
   - [ ] Test all monitoring features (HTTP/Ping/TCP)
   - [ ] Verify hotkey works (Ctrl+Shift+O)
   - [ ] Check system tray functionality
   - [ ] Test settings persistence

2. **Test the installer**
   - [ ] Install on clean Windows machine
   - [ ] Verify Start Menu shortcuts
   - [ ] Test uninstaller
   - [ ] Check if it asks to delete settings on uninstall

3. **Create GitHub Release**
   - [ ] Tag version (e.g., v1.0.0)
   - [ ] Upload `GhostOverlay-Setup-v1.0.0.exe` (installer)
   - [ ] Upload `GhostOverlay-v1.0.0-win-x64.zip` (portable)
   - [ ] Write release notes

4. **Update README.md**
   - [ ] Add download links
   - [ ] Update installation instructions

---

## Recommended Distribution Method

**For most users:** Use Inno Setup installer
- Professional appearance
- Easy one-click installation
- Auto-updates support
- Clean uninstall

**For advanced users:** Provide portable ZIP
- No installation required
- Run from USB drive
- Useful for testing

**Both options together:** Upload both to GitHub Releases
```
Releases
├── GhostOverlay-Setup-v1.0.0.exe  (Installer - 155 MB)
└── GhostOverlay-Portable-v1.0.0.zip  (Portable - 155 MB)
```

---

## Next Steps

1. ✅ `.exe` is ready at: `bin\Release\net8.0-windows\win-x64\publish\GhostOverlay.exe`
2. ⏭️ Download Inno Setup: https://jrsoftware.org/isdl.php
3. ⏭️ Open `installer\GhostOverlay-Setup.iss` in Inno Setup Compiler
4. ⏭️ Press F9 to build installer
5. ⏭️ Test installer: `installer\Output\GhostOverlay-Setup-v1.0.0.exe`
6. ⏭️ Create GitHub Release with both files

---

## File Locations

```
GhostOverlay_recovered/
├── bin/Release/net8.0-windows/win-x64/publish/
│   └── GhostOverlay.exe              ← Your ready-to-use executable
├── installer/
│   ├── GhostOverlay-Setup.iss        ← Inno Setup script
│   └── Output/                       ← Installer will be created here
└── BUILD_INSTRUCTIONS.md             ← This file
```
