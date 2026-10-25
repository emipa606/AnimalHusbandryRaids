# GitHub Copilot Instructions for Animal Husbandry Raids (Continued) Mod

## Mod Overview and Purpose
The Animal Husbandry Raids (Continued) mod is an update to the O Negatives mod. It adds animals from an internal list randomly during raids and allows exceptions to be specified in config files. The mod enhances the dynamic feel of factions by making them bring domesticated animals during raids, adding an extra layer of complexity and distraction to the gameplay.

## Key Features and Systems
The mod introduces the following key features and systems:

1. **Animal Definition Management**: The mod allows users to add or remove animal definitions from a static list through config files. The valid factions include Empire, Outlanders, Tribal, and Pirate, each with their own set of animal definitions.
2. **Random Animal Selection**: Animals are randomly selected from the static list and added to raids. This adds an element of surprise and variety to the raids.
3. **Configurable Exceptions**: Users can specify exceptions in config files, allowing specific animal definitions to be excluded from raids.
4. **Faction Venerated Animals**: The mod can spawn the faction's venerated animals during raids, further enhancing the faction's dynamic feel.

## Coding Patterns and Conventions
To ensure consistency and maintainability, follow these coding patterns and conventions:

1. **Namespace and File Structure**: Use the namespace `AnimalHusbandryRaids` for all C# code files. Keep the file structure organized, with initialization, settings, faction animal list management, and incident generation code separated into distinct files.
2. **Code Comments**: Write clear and concise comments for methods, properties, and classes to explain their purpose and functionality.
3. **Error Handling**: Implement error handling where necessary to manage unexpected situations and provide meaningful error messages.

## XML Integration
XML files are used to define patches for compatibility with other mods. Follow these guidelines for XML integration:

1. **Patch Definitions**: Define patches in XML files to modify the behavior of RimWorld's core or other mods. Ensure that the patch definitions are correctly formatted and referenced in the mod's main XML file.
2. **Mod Dependencies**: Specify mod dependencies in the XML files to ensure compatibility and proper loading order. Refer to the Semantic Summary for a list of dependencies.

## Harmony Patching
Harmony patching is used to modify the behavior of RimWorld's core or other mods. Follow these guidelines for Harmony patching:

1. **Patch Prefix/Postfix**: Use Harmony prefix and postfix methods to modify the behavior of existing methods. Ensure that the patch methods are correctly annotated and referenced in the XML patch definitions.
2. **Patch Method Naming**: Name patch methods clearly to indicate their purpose and functionality. Use PascalCase naming convention for patch methods.

## Suggestions for Copilot
To improve the mod's functionality and maintainability, consider the following suggestions for Copilot:

1. **Code Optimization**: Review and optimize existing code to improve performance and reduce memory usage. Pay special attention to the animal selection and list management logic.
2. **Mod Compatibility**: Ensure compatibility with other popular mods by testing the mod with various combinations and reporting any issues.
3. **User Documentation**: Improve the mod's user documentation by providing detailed instructions for configuring the mod and using the config files.
4. **Error Handling**: Enhance error handling by providing more informative error messages and logging mechanisms.

By following these guidelines and suggestions, you can contribute to the development and improvement of the Animal Husbandry Raids (Continued) mod, making it an even more enjoyable and dynamic experience for players.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

