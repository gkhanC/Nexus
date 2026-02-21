
import os

def get_base_names(directory, extensions):
    base_names = set()
    for root, dirs, files in os.walk(directory):
        for file in files:
            if any(file.endswith(ext) for ext in extensions):
                name = file
                for ext in extensions:
                    if name.endswith(ext):
                        name = name[:-len(ext)]
                        break
                # Handle Docs with _tr and _eng
                if name.endswith("_tr"): name = name[:-3]
                if name.endswith("_eng"): name = name[:-4]
                base_names.add(name)
    return base_names

source_dir = "/home/gokhanc/Development/Nexus"
docs_dir = "/home/gokhanc/Development/Nexus/Documents/API_References"

# Exclude Tests, Plugins, obj, bin
exclude_dirs = ["Plugins", "obj", "bin", "Nexus.Tests", "Nexus.UnityHelper.Tests"]

cs_files = {}
for root, dirs, files in os.walk(source_dir):
    if any(ex in root for ex in exclude_dirs):
        continue
    for file in files:
        if file.endswith(".cs") and file not in ["Class1.cs", "Program.cs"]:
            name = file[:-3]
            if name not in cs_files:
                cs_files[name] = []
            cs_files[name].append(os.path.join(root, file))

doc_names = get_base_names(docs_dir, [".md"])

undocumented = []
for name in cs_files:
    if name not in doc_names:
        undocumented.append(name)

undocumented.sort()
print("\n".join(undocumented))
