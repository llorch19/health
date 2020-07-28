using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
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

        public interface ISelfValidatableModel
        {
            string Validate();
        }

        public class ZFModelValidator<T> : IModelValidator where T : ISelfValidatableModel
        {
            public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
            {
                var model = (T)context.Model;
                var error = model.Validate();

                if (!string.IsNullOrEmpty(error))
                {
                    return new List<ModelValidationResult>{new ModelValidationResult(model.ToString(), error)};
                }
                return Enumerable.Empty<ModelValidationResult>();
            }
        }
    }
}
