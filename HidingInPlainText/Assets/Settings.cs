using UnityEngine;
using System.Collections;

public static class Settings
{
	const float MOUSE_SENSITIVITY_MIN = 1000.0f;
	const float MOUSE_SENSITIVITY_MAX = 10000.0f;
	const float MOUSE_SENSITIVITY_DEFAULT = 5000.0f;
	public static float MouseSensitivity { get; private set; }
	public static void SetMouseSensitivity(float value)
	{
		MouseSensitivity = Mathf.Clamp(value, MOUSE_SENSITIVITY_MIN, MOUSE_SENSITIVITY_MAX);
	}


	public static void Reset()
	{
		MouseSensitivity = MOUSE_SENSITIVITY_DEFAULT;
	}
}