﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Parameters
{
    public const float DESIRED_SPEED = 1f;
    public const float T = 0.5f;
    public const float A = 2000f;
    public const float B = 0.08f;
    public const float K = 1.2E5f;
    public const float KAPPA = 2.4E5f;

    public const float WALL_A = 2000f;
    public const float WALL_B = 0.08f;
    public const float WALL_K = 1.2E5f;
    public const float WALL_KAPPA = 2.4E5f;

    public const float TAN_A = 200f;
    public const float TAN_B = 0.08f;

    public const float MAX_ACCEL = float.MaxValue;
}