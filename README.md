# LazyFramework.DX 🚀

**Supercharge your UiPath development experience with LazyFramework.DX!** This package uses the power of UiPath's Studio SDK to provide delightful design-time tools for developers. Let’s be real: debugging and managing configuration files can be *painful*. LazyFramework.DX aims to make your life easier, and dare we say, fun!

## 🌟 Features and Modules

### **1. Hermes** 📨
Your all-seeing, all-knowing log manager. Hermes is the messenger god, and just like its namesake, it delivers logs in style.  
- 🕵️‍♀️ Filter logs by **level** (Info, Error) or **context** (specific services/modules).  
- 🔍 Perform lightning-fast text searches across logs.  
- ✨ A clean, developer-friendly UI to monitor activity. (See the attached screenshot for a sneak peek.)

**TL;DR:** No more wading through a sea of logs. Hermes keeps it neat and tidy for you.

---

### **2. Odin** 👁️
The wise overseer of your file system. Odin is a file-watching service that other modules can rely on for detecting file changes.  
- Monitors directories for events like file creation, updates, or deletions.  
- Powers **Athena** and other upcoming features by keeping them in sync with your project files.

**TL;DR:** Think of Odin as your project's vigilant sentinel.

---

### **3. Athena** 📚  
Say goodbye to dictionary chaos! UiPath uses Excel or JSON for configs, but without type-safety, mistakes are easy to make. Athena steps in to fix that.  
- Automatically generates **strongly-typed classes** from config files.  
- Uses Odin to watch for changes in your source config files and updates classes in real-time.

**TL;DR:** Athena ensures your config management is smarter, safer, and hassle-free.

---

### **4. TBD** 🛠️
This is where you come in! We're cooking up a service to generate **markdown documentation** from `.xaml` files in your project. Here's what it will do:  
- Document namespaces, arguments, references, and related workflows.  
- Create **Mermaid diagrams** to visually represent workflow structures.  
- Help you keep track of test cases and interdependencies.  

But we need the perfect name! Something that screams “god of documentation” or “keeper of order.” Got any ideas? Drop them in the suggestions! 🌟

**Some ideas so far:**  
- **Hephaestus** (Greek god of craftsmanship and tools)  
- **Thoth** (Egyptian god of writing and wisdom)  
- **Vishvakarman** (Hindu god of creation and tools)  

---

## 🔧 Installation & Usage

1. Install the package via your preferred method (NuGet, package manager, etc.).  
2. Hook Hermes, Odin, and Athena into your UiPath project.  
3. Sit back and let LazyFramework.DX do the heavy lifting.

---

## 🎉 Why LazyFramework.DX?

Because you deserve tools that make you feel like a **developer god**. Debugging is boring. Configuration is tedious. Documentation is a slog. Let LazyFramework.DX take care of the grunt work, so you can focus on what matters: building amazing automations.

**Join us in redefining the UiPath developer experience!**
