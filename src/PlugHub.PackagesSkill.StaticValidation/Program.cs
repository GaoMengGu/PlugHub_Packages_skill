using System.Text;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;
return RepositoryValidator.Run(args);

internal static class RepositoryValidator
{
    private static readonly string[] SkillDirectories = ["plughub-package-authoring", "plughub-package-authoring-en"];
    private static readonly string[] AgentEntryFiles =
    [
        "SOUL.md",
        "SOUL.en-US.md",
        "AGENTS.md",
        "AGENTS.en-US.md",
        "IDENTITY.md",
        "IDENTITY.en-US.md",
        "HERMES.md",
        "OPENCLAW.md",
        "TRAE.md",
        "CODEBUDDY.md"
    ];
    private static readonly string[] SensitiveFragments =
    [
        string.Concat("C:", "\\", "Users"),
        string.Concat("C:", "/", "Users"),
        string.Concat("Y", "ilan"),
        string.Concat(".codex", "\\", "skills", "\\", ".system"),
        string.Concat(".codex", "/", "skills", "/", ".system"),
        string.Concat("D:", "\\", "AI", "\\", "code"),
        string.Concat("D:", "/", "AI", "/", "code")
    ];

    private static readonly string LegacyScriptName = string.Concat("validate_plughub", "_package.py");
    private static readonly string LegacyRuntimeName = string.Concat("py", "thon");

    public static int Run(string[] args)
    {
        var root = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
        var errors = new List<string>();

        if (!Directory.Exists(root))
        {
            errors.Add($"Repository root does not exist: {root}");
            return Finish(errors);
        }

        ValidateRequiredFiles(root, errors);
        ValidateReadmes(root, errors);
        ValidateAgentEntries(root, errors);
        ValidateSkills(root, errors);
        ValidateSkillsJson(root, errors);
        ValidateWorkflow(root, errors);
        ValidateNoLegacyScripts(root, errors);
        ValidateNoTextLeaks(root, errors);

        return Finish(errors);
    }

    private static void ValidateRequiredFiles(string root, List<string> errors)
    {
        foreach (var relativePath in new[]
        {
            "README.md",
            "README.zh-CN.md",
            "README.en-US.md",
            "AGENTS.md",
            "skills.json",
            ".github/workflows/sync-gitee.yml"
        }.Concat(AgentEntryFiles))
        {
            RequireFile(root, relativePath, errors);
        }
    }

    private static void ValidateReadmes(string root, List<string> errors)
    {
        var rootReadme = ReadRequiredText(root, "README.md", errors);
        if (rootReadme.Length > 0)
        {
            RequireContains(rootReadme, "README.en-US.md", "README.md must link to the English README.", errors);
            RequireContains(rootReadme, "skills.json", "README.md must mention skills.json discovery.", errors);
            RejectContains(rootReadme, "dotnet run", "README.md must not document validation commands.", errors);
            RejectContains(rootReadme, "PlugHub.PackageValidator", "README.md must not document validator project usage.", errors);
        }

        var chinese = ReadRequiredText(root, "README.zh-CN.md", errors);
        if (chinese.Length > 0)
        {
            RequireContains(chinese, "dotnet run --project", "Chinese README must document C# validation.", errors);
            RequireContains(chinese, "plughub-package-authoring", "Chinese README must list the Chinese skill path.", errors);
            RequireContains(chinese, "plughub-package-authoring-en", "Chinese README must list the English skill path.", errors);
        }

        var english = ReadRequiredText(root, "README.en-US.md", errors);
        if (english.Length > 0)
        {
            RequireContains(english, "dotnet run --project", "English README must document C# validation.", errors);
            RequireContains(english, "plughub-package-authoring", "English README must list the Chinese skill path.", errors);
            RequireContains(english, "plughub-package-authoring-en", "English README must list the English skill path.", errors);
        }
    }

    private static void ValidateAgentEntries(string root, List<string> errors)
    {
        var soul = ReadRequiredText(root, "SOUL.md", errors);
        if (soul.Length > 0)
        {
            RequireContains(soul, "身份", "SOUL.md must include the 身份 keyword.", errors);
            RequireContains(soul, "记忆", "SOUL.md must include the 记忆 keyword.", errors);
            RequireContains(soul, "identity", "SOUL.md must include the identity keyword.", errors);
            RequireContains(soul, "communication", "SOUL.md must include the communication keyword.", errors);
            RequireContains(soul, "style", "SOUL.md must include the style keyword.", errors);
            RequireContains(soul, "规则", "SOUL.md must include the 规则 keyword.", errors);
            RequireContains(soul, "rules", "SOUL.md must include the rules keyword.", errors);
        }

        var soulEnglish = ReadRequiredText(root, "SOUL.en-US.md", errors);
        if (soulEnglish.Length > 0)
        {
            RequireContains(soulEnglish, "Identity", "SOUL.en-US.md must include the Identity keyword.", errors);
            RequireContains(soulEnglish, "Memory", "SOUL.en-US.md must include the Memory keyword.", errors);
            RequireContains(soulEnglish, "Communication", "SOUL.en-US.md must include the Communication keyword.", errors);
            RequireContains(soulEnglish, "Style", "SOUL.en-US.md must include the Style keyword.", errors);
            RequireContains(soulEnglish, "Rules", "SOUL.en-US.md must include the Rules keyword.", errors);
        }

        var identity = ReadRequiredText(root, "IDENTITY.md", errors);
        if (identity.Length > 0)
        {
            RequireSingleSentence(identity, "IDENTITY.md", errors);
            RequireContains(identity, "PlugHub", "IDENTITY.md must identify PlugHub.", errors);
            RequireContains(identity, "Revit 2020", "IDENTITY.md must preserve the Revit 2020 boundary.", errors);
        }

        var identityEnglish = ReadRequiredText(root, "IDENTITY.en-US.md", errors);
        if (identityEnglish.Length > 0)
        {
            RequireSingleSentence(identityEnglish, "IDENTITY.en-US.md", errors);
            RequireContains(identityEnglish, "PlugHub", "IDENTITY.en-US.md must identify PlugHub.", errors);
            RequireContains(identityEnglish, "Revit 2020", "IDENTITY.en-US.md must preserve the Revit 2020 boundary.", errors);
        }

        var agents = ReadRequiredText(root, "AGENTS.md", errors);
        if (agents.Length > 0)
        {
            RequireContains(agents, "使命", "AGENTS.md must include the 使命 keyword.", errors);
            RequireContains(agents, "mission", "AGENTS.md must include the mission keyword.", errors);
            RequireContains(agents, "交付", "AGENTS.md must include the 交付 keyword.", errors);
            RequireContains(agents, "workflow", "AGENTS.md must include the workflow keyword.", errors);
            RequireContains(agents, "IDENTITY.md", "AGENTS.md must load IDENTITY.md.", errors);
            RequireContains(agents, "SOUL.md", "AGENTS.md must load SOUL.md.", errors);
            RequireContains(agents, "skills.json", "AGENTS.md must use skills.json discovery.", errors);
            RequireContains(agents, "Hermes", "AGENTS.md must mention Hermes.", errors);
            RequireContains(agents, "OpenClaw", "AGENTS.md must mention OpenClaw.", errors);
        }

        var agentsEnglish = ReadRequiredText(root, "AGENTS.en-US.md", errors);
        if (agentsEnglish.Length > 0)
        {
            RequireContains(agentsEnglish, "Mission", "AGENTS.en-US.md must include the Mission keyword.", errors);
            RequireContains(agentsEnglish, "Workflow", "AGENTS.en-US.md must include the Workflow keyword.", errors);
            RequireContains(agentsEnglish, "Delivery", "AGENTS.en-US.md must include the Delivery keyword.", errors);
            RequireContains(agentsEnglish, "IDENTITY.en-US.md", "AGENTS.en-US.md must load IDENTITY.en-US.md.", errors);
            RequireContains(agentsEnglish, "SOUL.en-US.md", "AGENTS.en-US.md must load SOUL.en-US.md.", errors);
            RequireContains(agentsEnglish, "skills.json", "AGENTS.en-US.md must use skills.json discovery.", errors);
        }
    }

    private static void ValidateSkills(string root, List<string> errors)
    {
        foreach (var skillDirectory in SkillDirectories)
        {
            var skillRoot = Path.Combine(root, skillDirectory);
            if (!Directory.Exists(skillRoot))
            {
                errors.Add($"Missing skill directory: {skillDirectory}");
                continue;
            }

            ValidateSkillFrontmatter(root, skillDirectory, errors);
            RequireFile(root, $"{skillDirectory}/agents/openai.yaml", errors);
            RequireFile(root, $"{skillDirectory}/references/plughub-package-contract.md", errors);
            RequireFile(root, $"{skillDirectory}/references/authoring-playbook.md", errors);
            RequireFile(root, $"{skillDirectory}/references/agent-compatibility.md", errors);
            RequireFile(root, $"{skillDirectory}/tools/PlugHub.PackageValidator/PlugHub.PackageValidator.csproj", errors);
            RequireFile(root, $"{skillDirectory}/tools/PlugHub.PackageValidator/Program.cs", errors);

            var skillText = ReadRequiredText(root, $"{skillDirectory}/SKILL.md", errors);
            RequireContains(skillText, "tools/PlugHub.PackageValidator/PlugHub.PackageValidator.csproj", $"{skillDirectory}/SKILL.md must reference the C# package validator.", errors);

            var playbook = ReadRequiredText(root, $"{skillDirectory}/references/authoring-playbook.md", errors);
            RequireContains(playbook, "tools/PlugHub.PackageValidator/PlugHub.PackageValidator.csproj", $"{skillDirectory}/authoring-playbook.md must reference the C# package validator.", errors);

            var compatibility = ReadRequiredText(root, $"{skillDirectory}/references/agent-compatibility.md", errors);
            foreach (var agent in new[] { "Codex", "Hermes", "OpenClaw", "Trae", "CodeBuddy" })
            {
                RequireContains(compatibility, agent, $"{skillDirectory}/agent-compatibility.md must mention {agent}.", errors);
            }
        }
    }

    private static void ValidateSkillFrontmatter(string root, string skillDirectory, List<string> errors)
    {
        var text = ReadRequiredText(root, $"{skillDirectory}/SKILL.md", errors);
        if (text.Length == 0)
        {
            return;
        }

        var lines = text.Replace("\r\n", "\n").Split('\n');
        if (lines.Length < 4 || lines[0] != "---")
        {
            errors.Add($"{skillDirectory}/SKILL.md must start with YAML frontmatter.");
            return;
        }

        var end = Array.FindIndex(lines, 1, line => line == "---");
        if (end < 0)
        {
            errors.Add($"{skillDirectory}/SKILL.md frontmatter is not closed.");
            return;
        }

        var keys = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 1; index < end; index++)
        {
            var line = lines[index];
            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
            {
                errors.Add($"{skillDirectory}/SKILL.md frontmatter line {index + 1} is not a key/value pair.");
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            keys[key] = value;
        }

        foreach (var key in keys.Keys.Where(key => key is not "name" and not "description"))
        {
            errors.Add($"{skillDirectory}/SKILL.md frontmatter must only use name and description, found: {key}");
        }

        if (!keys.TryGetValue("name", out var name) || name != skillDirectory)
        {
            errors.Add($"{skillDirectory}/SKILL.md frontmatter name must match the directory.");
        }

        if (!keys.TryGetValue("description", out var description) || description.Length == 0)
        {
            errors.Add($"{skillDirectory}/SKILL.md frontmatter description is required.");
        }
    }

    private static void ValidateSkillsJson(string root, List<string> errors)
    {
        var path = Path.Combine(root, "skills.json");
        if (!File.Exists(path))
        {
            return;
        }

        using var document = ReadJson(path, errors);
        if (document is null)
        {
            return;
        }

        var rootElement = document.RootElement;
        RequireJsonString(rootElement, "repository", "GaoMengGu/PlugHub_Packages_skill", "skills.json repository must match this repository.", errors);

        if (!rootElement.TryGetProperty("skills", out var skills) || skills.ValueKind != JsonValueKind.Array)
        {
            errors.Add("skills.json must contain a skills array.");
            return;
        }

        foreach (var skillDirectory in SkillDirectories)
        {
            var found = skills.EnumerateArray().Any(item =>
                item.ValueKind == JsonValueKind.Object &&
                item.TryGetProperty("name", out var name) &&
                item.TryGetProperty("path", out var pathValue) &&
                name.GetString() == skillDirectory &&
                pathValue.GetString() == skillDirectory);

            if (!found)
            {
                errors.Add($"skills.json must list {skillDirectory} with matching name and path.");
            }
        }
    }

    private static void ValidateWorkflow(string root, List<string> errors)
    {
        var workflow = ReadRequiredText(root, ".github/workflows/sync-gitee.yml", errors);
        if (workflow.Length == 0)
        {
            return;
        }

        RequireContains(workflow, "push:", "Gitee sync workflow must run on push.", errors);
        RequireContains(workflow, "main", "Gitee sync workflow must target main.", errors);
        RequireContains(workflow, "workflow_dispatch:", "Gitee sync workflow must support manual dispatch.", errors);
        RequireContains(workflow, "gitee.com:GaoMengGu/PlugHub_Packages_skill.git", "Gitee sync workflow must push to the expected Gitee repository.", errors);
        RequireContains(workflow, "GITEE_PRIVATE_KEY", "Gitee sync workflow must use the GITEE_PRIVATE_KEY secret.", errors);
    }

    private static void ValidateNoLegacyScripts(string root, List<string> errors)
    {
        var legacyFiles = Directory.EnumerateFiles(root, "*.py", SearchOption.AllDirectories)
            .Where(path => !IsIgnoredPath(root, path))
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToList();

        foreach (var file in legacyFiles)
        {
            errors.Add($"Remove legacy .py file: {file}");
        }
    }

    private static void ValidateNoTextLeaks(string root, List<string> errors)
    {
        foreach (var file in EnumeratePolicyTextFiles(root))
        {
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            var text = File.ReadAllText(file, Encoding.UTF8);

            foreach (var fragment in SensitiveFragments)
            {
                if (text.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"{relative} contains a machine-specific path or personal fragment.");
                }
            }

            if (text.Contains(LegacyScriptName, StringComparison.OrdinalIgnoreCase) ||
                text.Contains(LegacyRuntimeName, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"{relative} still references the legacy script runtime.");
            }
        }
    }

    private static IEnumerable<string> EnumeratePolicyTextFiles(string root)
    {
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".md",
            ".json",
            ".yaml",
            ".yml",
            ".csproj"
        };

        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !IsIgnoredPath(root, path))
            .Where(path => allowedExtensions.Contains(Path.GetExtension(path)));
    }

    private static JsonDocument? ReadJson(string path, List<string> errors)
    {
        try
        {
            return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            errors.Add($"{Path.GetFileName(path)} is not valid JSON: {ex.Message}");
            return null;
        }
    }

    private static void RequireJsonString(JsonElement root, string propertyName, string expected, string error, List<string> errors)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String || value.GetString() != expected)
        {
            errors.Add(error);
        }
    }

    private static void RequireFile(string root, string relativePath, List<string> errors)
    {
        if (!File.Exists(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar))))
        {
            errors.Add($"Missing required file: {relativePath}");
        }
    }

    private static string ReadRequiredText(string root, string relativePath, List<string> errors)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            errors.Add($"Missing required file: {relativePath}");
            return string.Empty;
        }

        return File.ReadAllText(path, Encoding.UTF8);
    }

    private static void RequireContains(string text, string expected, string error, List<string> errors)
    {
        if (!text.Contains(expected, StringComparison.Ordinal))
        {
            errors.Add(error);
        }
    }

    private static void RejectContains(string text, string forbidden, string error, List<string> errors)
    {
        if (text.Contains(forbidden, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(error);
        }
    }

    private static void RequireSingleSentence(string text, string relativePath, List<string> errors)
    {
        var normalized = text.Trim();
        var nonEmptyLines = normalized.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (nonEmptyLines.Length != 1)
        {
            errors.Add($"{relativePath} must be a single non-empty line.");
        }

        var sentenceTerminators = normalized.Count(ch => ch is '.' or '。' or '!' or '！' or '?' or '？');
        if (sentenceTerminators != 1)
        {
            errors.Add($"{relativePath} must contain exactly one sentence terminator.");
        }
    }

    private static bool IsIgnoredPath(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
        var parts = relative.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return relative == ".git" ||
            relative.StartsWith(".git/", StringComparison.Ordinal) ||
            parts.Contains("bin", StringComparer.OrdinalIgnoreCase) ||
            parts.Contains("obj", StringComparer.OrdinalIgnoreCase);
    }

    private static int Finish(List<string> errors)
    {
        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                Console.WriteLine($"ERROR: {error}");
            }

            Console.WriteLine($"PlugHub_Packages_skill static validation failed with {errors.Count} error(s).");
            return 1;
        }

        Console.WriteLine("PlugHub_Packages_skill static validation passed.");
        return 0;
    }
}
