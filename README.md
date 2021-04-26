# CS3211 Project - Applying Probabilistic Model Checking in Soccer Analytics
This repo contains the CSP# model and the relevant files for NUS's CS3211 project for AY20/21 SEM2. This project uses the [PAT model checker](https://pat.comp.nus.edu.sg/) to apply probabilistic model checking in soccer analytics.

## File Structure
- `soccer-zone-model.pcsp` contains the model in CSP#.
- `Pat.Lib.SoccerStateHelper.cs` and `geometryUtils.cs` are the helper C# libraries used in the CSP# model. `geometryUtils.cs` is used directly in `Pat.Lib.SoccerStateHelper.cs`.
- `Pat.Lib.SoccerStateHelper.dll` is the built C# libary.
- `scripts` is the folder containing scripted used in our project.

## Usage
1. Download `PAT 3` from https://pat.comp.nus.edu.sg/.
2. Clone the repo.
3. Open `soccer-zone-model.pcsp` using `PAT 3`.
4. Edit control variables and perform verifications.  