import os
import glob

base_dir = "/home/gokhanc/Development/Nexus/Documents"

def get_md_files(subdir):
    files = glob.glob(os.path.join(base_dir, subdir, "*.md"))
    return sorted([os.path.relpath(f, base_dir).replace('\\', '/') for f in files])

nav_str = """site_name: Nexus Prime
site_description: High-Performance Data-Oriented ECS Framework for Unity
site_url: https://gokhanc.github.io/Nexus/
docs_dir: Documents

theme:
  name: material
  font:
    text: Roboto
    code: Roboto Mono
  features:
    - navigation.sections
    - navigation.expand
    - navigation.top
    - navigation.instant
    - navigation.tracking
    - search.suggest
    - search.highlight
    - content.code.copy
  palette:
    - media: "(prefers-color-scheme: dark)"
      scheme: slate
      primary: deep purple
      accent: cyan
      toggle:
        icon: material/weather-night
        name: Switch to light mode
    - media: "(prefers-color-scheme: light)"
      scheme: default
      primary: indigo
      accent: blue
      toggle:
        icon: material/weather-sunny
        name: Switch to dark mode

markdown_extensions:
  - admonition
  - tables
  - attr_list
  - md_in_html
  - toc:
      permalink: true
  - pymdownx.details
  - pymdownx.superfences:
      custom_fences:
        - name: mermaid
          class: mermaid
          format: !!python/name:pymdownx.superfences.fence_code_format
  - pymdownx.arithmatex:
      generic: true
  - pymdownx.highlight:
      anchor_linenums: true
  - pymdownx.inlinehilite
  - pymdownx.snippets
  - pymdownx.betterem

extra_javascript:
  - javascripts/mathjax.js
  - https://polyfill.io/v3/polyfill.min.js?features=es6
  - https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js

nav:
  - "Ana Sayfa": 'index.md'
  - "Manifesto":
      - "Nexus": 'Manifesto/Nexus.md'
      - "White Paper (Raw)": 'Manifesto/Nexus_WhitePaper.md'
  - "Tutorial & Mastery": 'Tutorials/Tutorial.md'
"""

with open("/home/gokhanc/Development/Nexus/mkdocs.yml", "w", encoding="utf-8") as f:
    f.write(nav_str)
    
    # Manuals
    f.write('  - "Manuals (Kullanım Kılavuzları)":\n')
    for mf in get_md_files("Manuals"):
        f.write(f'      - "{os.path.basename(mf).replace(".md", "")}": \'{mf}\'\n')

    # Core Modules
    f.write('  - "Core Modules":\n')
    for cmf in get_md_files("Core_Modules"):
        f.write(f'      - "{os.path.basename(cmf).replace(".md", "")}": \'{cmf}\'\n')

    # API References
    f.write('  - "API References":\n')
    for apif in get_md_files("API_References"):
        f.write(f'      - "{os.path.basename(apif).replace(".md", "")}": \'{apif}\'\n')

print("Modernized mkdocs.yml successfully generated.")
