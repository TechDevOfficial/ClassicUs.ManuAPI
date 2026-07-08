# ManuAPI Docs

This branch is served via GitHub Pages. The compiled static site lives at the
root of this branch; the Docusaurus source lives in `source/`.

To publish an update:

```
cd source
npm install
npm run build
```

then copy the contents of `source/build/` to the root of this branch
(everything except `source/` itself) and push.
