using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI.Attributes
{
    public class Vector3FieldAttribute : FieldBaseAttribute
    {
        public bool IsRestrict { get { return MaxXValue > 0 && MaxYValue > 0 && MaxZValue > 0; } }
        public float DefaultValueUniform
        {
            get
            {
                return DefaultXValue;
            }
            set
            {
                DefaultXValue =
                DefaultYValue =
                DefaultZValue = value;
            }
        }

        public float DefaultXValue { get; set; }
        public float DefaultYValue { get; set; }
        public float DefaultZValue { get; set; }

        public float MinXValue { get; set; } = 0;
        public float MinYValue { get; set; } = 0;
        public float MinZValue { get; set; } = 0;
        public float MaxXValue { get; set; } = -1;
        public float MaxYValue { get; set; } = -1;
        public float MaxZValue { get; set; } = -1;


        public float MinValueUniform
        {
            get
            {
                return MinXValue;
            }
            set
            {
                MinXValue =
                MinYValue =
                MinZValue = value;
            }
        }

        public float MaxValueUniform
        {
            get
            {
                return MaxXValue;
            }
            set
            {
                MaxXValue =
                MaxYValue =
                MaxZValue = value;
            }
        }
    }
}
