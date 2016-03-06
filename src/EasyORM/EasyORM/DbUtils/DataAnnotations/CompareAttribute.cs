using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace EasyORM.DbUtils.DataAnnotations
{
    public class CompareAttribute : ValidationAttribute
    {
        private string _otherProperty;
        public CompareAttribute(string otherProperty)
        {
            this._otherProperty = otherProperty;
        }

        public override bool IsValid(object value)
        {
            return base.IsValid(value);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(_otherProperty);
            var otherValue = propertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (object.Equals(value, otherValue))
            {
                return ValidationResult.Success;
            }
            if (value.ToString() == otherValue.ToString())
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("xx");
        }
    }
}
