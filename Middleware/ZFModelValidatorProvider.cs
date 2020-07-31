using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace health.Middleware
{
    public class ZFModelValidatorProvider : IModelValidatorProvider
    {
        public void CreateValidators(ModelValidatorProviderContext context)
        {
            if (context.ModelMetadata.MetadataKind == ModelMetadataKind.Type)
            {
                var validatorType = typeof(ZFModelValidator<>)
                    .MakeGenericType(context.ModelMetadata.ModelType);
                var validator = (IModelValidator)Activator.CreateInstance(validatorType);

                context.Results.Add(new ValidatorItem
                {
                    Validator = validator,
                    IsReusable = true
                });
            }
        }


        public class ZFModelValidator<T> : IModelValidator 
        {
            public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
            {
                return Enumerable.Empty<ModelValidationResult>();
            }
        }
    }
}
