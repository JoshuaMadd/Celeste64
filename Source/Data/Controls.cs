using System.Diagnostics;

namespace Celeste64;

public static class Controls
{
	public static readonly VirtualStick Move = new("Move", VirtualAxis.Overlaps.TakeNewer, 0.35f);
	public static readonly VirtualStick Menu = new("Menu", VirtualAxis.Overlaps.TakeNewer, 0.35f);
	public static readonly VirtualStick Camera = new("Camera", VirtualAxis.Overlaps.TakeNewer, 0.35f);
	public static readonly VirtualButton Jump = new("Jump", .1f);
	public static readonly VirtualButton Dash = new("Dash", .1f);
	public static readonly VirtualButton Climb = new("Climb");
	public static readonly VirtualButton Confirm = new("Confirm");
	public static readonly VirtualButton Cancel = new("Cancel");
	public static readonly VirtualButton Pause = new("Pause");
	public static ControlsConfig Config = ControlsConfig.Defaults;

	public static void Load(ControlsConfig? config = null)
	{
		Config = config;
		static ControlsConfig.Stick FindStick(ControlsConfig? config, string name)
		{
			if (config != null && config.Sticks.TryGetValue(name, out var stick))
				return stick;
			if (ControlsConfig.Defaults.Sticks.TryGetValue(name, out stick))
				return stick;
			throw new Exception($"Missing Stick Binding for '{name}'");
		}

		static List<ControlsConfig.Binding> FindAction(ControlsConfig? config, string name)
		{
			if (config != null && config.Actions.TryGetValue(name, out var action))
				return action;
			if (ControlsConfig.Defaults.Actions.TryGetValue(name, out action))
				return action;
			throw new Exception($"Missing Action Binding for '{name}'");
		}

		Clear();

		FindStick(config, "Move").BindTo(Move);
		FindStick(config, "Camera").BindTo(Camera);
		FindStick(config, "Menu").BindTo(Menu);

		foreach (var it in FindAction(config, "Jump"))
			it.BindTo(Jump);
		foreach (var it in FindAction(config, "Dash"))
			it.BindTo(Dash);
		foreach (var it in FindAction(config, "Climb"))
			it.BindTo(Climb);
		foreach (var it in FindAction(config, "Confirm"))
			it.BindTo(Confirm);
		foreach (var it in FindAction(config, "Cancel"))
			it.BindTo(Cancel);
		foreach (var it in FindAction(config, "Pause"))
			it.BindTo(Pause);

	}

	public static void Clear()
	{
		Move.Clear();
		Camera.Clear();
		Jump.Clear();
		Dash.Clear();
		Climb.Clear();
		Menu.Clear();
		Confirm.Clear();
		Cancel.Clear();
		Pause.Clear();
	}

	public static void Consume()
	{
		Move.Consume();
		Menu.Consume();
		Camera.Consume();
		Jump.Consume();
		Dash.Consume();
		Climb.Consume();
		Confirm.Consume();
		Cancel.Consume();
		Pause.Consume();
	}

	private static readonly Dictionary<string, Dictionary<string, string>> prompts = [];

	private static string GetControllerName(Gamepads pad) => pad switch
	{
		Gamepads.DualShock4 => "PlayStation 4",
		Gamepads.DualSense => "PlayStation 5",
		Gamepads.Nintendo => "Nintendo Switch",
		Gamepads.Xbox => "Xbox Series",
		_ => "Xbox Series",
	};

	private static List<string> GetPromptLocations(VirtualButton button)
	{
		List<string> locations = [];
		var gamepad = Input.Controllers[0];
		var deviceTypeName =
			gamepad.Connected ? GetControllerName(gamepad.Gamepad) : "PC";
		foreach (var binding in Config.Actions[button.Name])
		{
			string promptDeviceTypeName = "PC";
			var name = binding.GetBindingName();
			if (binding.IsForController())
				promptDeviceTypeName = GetControllerName(gamepad.Gamepad);

			if (!prompts.TryGetValue(deviceTypeName, out var list))
				prompts[deviceTypeName] = list = [];

			if (!list.TryGetValue(name, out var lookup))
				list[name] = lookup = $"Controls/{promptDeviceTypeName}/{name}";

			if(Gamepads.Nintendo.Equals(binding.NotFor) || !Gamepads.Nintendo.Equals(binding.OnlyFor) || (binding.NotFor == null && binding.OnlyFor == null)) //only non switch prompts atm
				locations.Add(lookup);

		}

		return locations;
	}

	private static string GetPromptLocation(VirtualButton button)
	{
		var gamepad = Input.Controllers[0];
		var deviceTypeName =
			gamepad.Connected ? GetControllerName(gamepad.Gamepad) : "PC";
		string name = GetFirstBinding(button).GetBindingName();

		if (!prompts.TryGetValue(deviceTypeName, out var list))
			prompts[deviceTypeName] = list = [];

		if (!list.TryGetValue(name, out var lookup))
			list[name] = lookup = $"Controls/{deviceTypeName}/{name}";

		return lookup;
	}

	private static ControlsConfig.Binding? GetFirstBinding(VirtualButton button)
	{
		ControlsConfig.Binding? binding = null;
		var gamepad = Input.Controllers[0];
		int i = 0;
		while (gamepad.Connected && binding == null && i < Config.Actions[button.Name].Count)
		{
			var b = Config.Actions[button.Name][i];
			if(b.IsForController())
				binding = b;
			i++;
		}
		i = 0;
		while (!gamepad.Connected && binding == null && i < Config.Actions[button.Name].Count)
		{
			var b = Config.Actions[button.Name][i];
			if(!b.IsForController())
				binding = b;
			i++;
		}
		return binding;
	}


	public static Subtexture GetPrompt(VirtualButton button)
	{
		return Assets.Subtextures.GetValueOrDefault(GetPromptLocation(button));
	}

	public static List<Subtexture> GetPrompts(VirtualButton button)
	{
		List<Subtexture> subtextures = [];
		foreach (var location in GetPromptLocations(button))
			subtextures.Add(Assets.Subtextures.GetValueOrDefault(location));
		return subtextures;
	}
}
