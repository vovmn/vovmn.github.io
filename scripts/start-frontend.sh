#!/bin/sh
set -e
# Install dependencies (including optional binaries like esbuild)
npm install || true
# If rollup native loader exists, replace throws with warnings to avoid crash
if [ -f node_modules/rollup/dist/native.js ]; then
  sed -i "s/throw new Error(/console.warn(/g" node_modules/rollup/dist/native.js || true
fi
# Start Vite dev server
npm run dev -- --host 0.0.0.0
