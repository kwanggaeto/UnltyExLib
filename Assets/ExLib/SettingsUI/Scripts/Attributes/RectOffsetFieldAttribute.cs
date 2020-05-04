using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib.SettingsUI.Attributes
{
    public class RectOffsetFieldAttribute : FieldBaseAttribute
    {
        public bool IsRestrict { get { return MaxLeftValue > 0 && MaxRightValue > 0 && MaxTopValue > 0 && MaxBottomValue > 0; } }
        public int DefaultValueUniform
        {
            get
            {
                return DefaultLeftValue;
            }
            set
            {
                DefaultLeftValue =
                DefaultRightValue =
                DefaultTopValue = 
                DefaultBottomValue = value;
            }
        }

        public int DefaultLeftValue { get; set; }
        public int DefaultRightValue { get; set; }
        public int DefaultTopValue { get; set; }
        public int DefaultBottomValue { get; set; }

        public int MinLeftValue { get; set; } = 0;
        public int MinRightValue { get; set; } = 0;
        public int MinTopValue { get; set; } = 0;
        public int MinBottomValue { get; set; } = 0;
        public int MaxLeftValue { get; set; } = -1;
        public int MaxRightValue { get; set; } = -1;
        public int MaxTopValue { get; set; } = -1;
        public int MaxBottomValue { get; set; } = -1;


        public int MinValueUniform
        {
            get
            {
                return MinLeftValue;
            }
            set
            {
                MinLeftValue =
                MinRightValue =
                MinTopValue = 
                MinBottomValue = value;
            }
        }

        public int MaxValueUniform
        {
            get
            {
                return MaxLeftValue;
            }
            set
            {
                MaxLeftValue =
                MaxRightValue =
                MaxTopValue = 
                MaxBottomValue = value;
            }
        }
    }
}
