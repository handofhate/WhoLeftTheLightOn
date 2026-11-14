#!/usr/bin/env python3
"""Builds the wiki from wiki/ into an output folder (default _wiki/): every page as it is, with each
<!-- include: CHANGELOG.md --> marker filled in with the changelog, so the wiki never holds a stale copy.
.github/workflows/wiki.yml runs this on every push to main and publishes the result to the GitHub wiki.
Run it yourself to look:  python3 tools/wiki.py [out]"""
import os, re, shutil, sys
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.abspath(sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, "_wiki"))
read = lambda p: open(os.path.join(ROOT, p), encoding="utf-8").read()

def include(m):
    what = m.group(1).strip()
    if what == "CHANGELOG.md": return read("CHANGELOG.md").rstrip()
    raise SystemExit(f"unknown include: {what}")

if os.path.isdir(OUT):
    for f in os.listdir(OUT):
        if f != ".git": (shutil.rmtree if os.path.isdir(os.path.join(OUT, f)) else os.remove)(os.path.join(OUT, f))
os.makedirs(OUT, exist_ok=True)
pages = 0
for f in sorted(os.listdir(os.path.join(ROOT, "wiki"))):
    if not f.endswith(".md"): continue
    text = re.sub(r"<!-- include: (.+?) -->", include, read(os.path.join("wiki", f)))
    open(os.path.join(OUT, f), "w", encoding="utf-8").write(text)
    pages += 1
print(f"{pages} pages -> {OUT}")
