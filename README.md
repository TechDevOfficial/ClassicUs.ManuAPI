# ManuAPI Docs — built site

This branch contains the compiled static output of the ManuAPI documentation site, served via GitHub Pages.

Source lives in the `docs/` folder on the `main` branch (Docusaurus). To publish an update:

```
cd docs
npm run build
```

then copy the contents of `docs/build/` to the root of this branch and push.
