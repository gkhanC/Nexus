import os
import glob
import time
import json
import urllib.request
import urllib.error

# ==========================================
# NEXUS PRIME: API DOCUMENTATION BATCHER
# This script processes the remaining 150+ API files autonomously.
# It sends the raw .md file to an LLM (OpenAI style endpoint) and
# demands a White Paper Standard rewrite with Mermaid & Math.
# ==========================================

# CONFIGURATION
TARGET_DIRECTORY = "/home/gokhanc/Development/Nexus/Documents/API_References"
API_KEY = os.environ.get("OPENAI_API_KEY", "YOUR_API_KEY_HERE")
API_URL = "https://api.openai.com/v1/chat/completions" # Or swap to local LLM / Gemini endpoint
MODEL_NAME = "gpt-4o" # Deep reasoning model required for C# pointer logic

SYSTEM_PROMPT = """
You are Antigravity, the lead architect for Nexus Prime (A high-performance ECS Framework in Unity/C#).
Your job is to rewrite the provided API Reference markdown file.
RULES:
1. DO NOT remove any existing information or source code.
2. If the user provided C# source code, break it down line-by-line explaining the unmanaged/cache mathematics behind it.
3. Inject GitHub-Flavored Markdown (GFM) Alerts (e.g., > [!WARNING]).
4. Provide concrete code usage scenarios with warnings regarding pointer drift or multithreading.
5. Embed a Mermaid js graph (`mermaid`) visualizing the API's execution flow.
6. Use LaTeX style math bounds (e.g., $O(1)$) to explain performance metrics.
7. Return ONLY the rewritten markdown content. No conversational intro.
"""

def call_llm(content_to_rewrite, file_name):
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {API_KEY}"
    }
    
    payload = {
        "model": MODEL_NAME,
        "messages": [
            {"role": "system", "content": SYSTEM_PROMPT},
            {"role": "user", "content": f"Rewrite this file ({file_name}) matching the Nexus White Paper standard:\n\n{content_to_rewrite}"}
        ],
        "temperature": 0.2
    }
    
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(API_URL, data=data, headers=headers, method="POST")
    
    try:
        with urllib.request.urlopen(req) as response:
            result = json.loads(response.read().decode("utf-8"))
            return result["choices"][0]["message"]["content"]
    except urllib.error.HTTPError as e:
        print(f"HTTP Error: {e.code} - {e.read().decode('utf-8')}")
        return None
    except Exception as e:
        print(f"Exception: {str(e)}")
        return None

def process_directory():
    if API_KEY == "YOUR_API_KEY_HERE":
        print("ERROR: Please set your OPENAI_API_KEY inside the script or via environment variables.")
        return

    md_files = glob.glob(os.path.join(TARGET_DIRECTORY, '*.md'))
    total = len(md_files)
    
    print(f"Found {total} Markdown files in API_References. Booting Nexus Engine Batcher...")
    
    # Skips already manually processed files from Phase 3
    skip_list = [
        "EntityId_eng.md", "EntityId_tr.md", 
        "EntityCommandBuffer_eng.md", "EntityCommandBuffer_tr.md",
        "ComponentTypeManager_eng.md", "ComponentTypeManager_tr.md",
        "ISparseSet_eng.md", "ISparseSet_tr.md"
    ]
    
    success_count = 0
    
    for idx, file_path in enumerate(md_files):
        file_name = os.path.basename(file_path)
        
        if file_name in skip_list:
            print(f"[{idx+1}/{total}] Skipping {file_name} (Already hand-crafted).")
            continue
            
        print(f"[{idx+1}/{total}] Processing {file_name}...")
        
        with open(file_path, 'r', encoding='utf-8') as f:
            original_content = f.read()
            
        # Prevent double-processing
        if "Mermaid" in original_content or "O(1)" in original_content:
            print(f"  -> File appears to be already enriched. Skipping.")
            continue
            
        new_content = call_llm(original_content, file_name)
        
        if new_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            print(f"  -> Success! Overwritten {file_name}.")
            success_count += 1
            time.sleep(1) # Prevent rate-limiting
        else:
            print(f"  -> FAILED to process {file_name}.")
            
    print(f"\nBatch Job Complete. Successfully enriched {success_count} files via LLM.")

if __name__ == "__main__":
    process_directory()
