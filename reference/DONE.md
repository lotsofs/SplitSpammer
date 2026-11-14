# GTA SA log rows (reference)

> Reference copy, moved from livesplit-asl's docs/DONE.md (the rows) on 2026-11-14 when GTA SA auto splitting moved to this repo. It describes `GTASA.asl` (kept beside it, git-ignored), not SplitSpammer's own work, and is not live tracking: SplitSpammer's open work is in `docs/TODO.md`.

| Date | Code | Area | Change | Why / how |
|---|---|---|---|---|
| 2026-11-14 | WSM | GTA SA | Try LiveSplit's auto-splitting runtime on the Firefighter mission with a Rust spike under gtasa/, and decide against it for GTA SA | The runtime's settings dialog is a flat table that can't collapse, which can't hold the ASL's hundreds of settings (read in its source and seen in a live test of the spike); GTA SA moves to its own LiveSplit component instead, tracked as SPS. The spike's tests replay traces recorded from the real ASL and its module built and loaded. |
| 2026-11-13 | PTO, HDR | GTA SA | Support only game version 1.0: drop the per-version offsets and the 1.01, 2.00 and Steam detection, and fix the header's address formulas | Only 1.0 (with SSP, which leaves the exe unchanged) is meant to be supported, and the pointer watchers never received the offset anyway. Other builds now read "Unsupported (use 1.0)" and run nothing. Not yet seen in LiveSplit (VER 26). |
| 2025-12-04 | — | GTA SA | Port Los Santos from Los Sepulcros to The Green Sabre and all of Badlands to the new split framework, with Trucking, the Chiliad Challenge, death and busted warps and five more phone calls | The rewrite's live code ended at High Stakes, Low-Rider; these missions existed only in the dead code. Each got a settings region, watchers and a check func, with a shorthand helper for the shared marker and start settings. Not yet recorded as seen working in game (VER). |
