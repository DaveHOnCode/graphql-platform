using System;
using HotChocolate.Language;

namespace HotChocolate.Types.Sorting;

[Obsolete("Use HotChocolate.Data.")]
public interface ISortObjectOperationDescriptor
    : IDescriptor<SortOperationDefintion>
    , IFluent
{
    /// <summary>
    /// Specify the name of the sort operation.
    /// </summary>
    /// <param name="value">
    ///  The sort operation name.
    /// </param>
    ISortObjectOperationDescriptor Name(string value);

    /// <summary>
    /// Ignore the specified property.
    /// </summary>
    /// <param name="ignore">If set to true the field is ignored</param>
    ISortObjectOperationDescriptor Ignore(bool ignore = true);

    /// <summary>
    /// Specify the description of the filter operation.
    /// </summary>
    /// <param name="value">
    ///  The operation description.
    /// </param>
    ISortObjectOperationDescriptor Description(string value);

    /// <summary>
    /// Annotate the operation filter field with a directive.
    /// </summary>
    /// <param name="directiveInstance">
    /// The directive with which the field shall be annotated.
    /// </param>
    /// <typeparam name="T">
    /// The directive type.
    /// </typeparam>
    ISortObjectOperationDescriptor Directive<T>(T directiveInstance)
        where T : class;

    /// <summary>
    /// Annotate the operation filter field with a directive.
    /// </summary>
    /// <typeparam name="T">
    /// The directive type.
    /// </typeparam>
    ISortObjectOperationDescriptor Directive<T>()
        where T : class, new();

    /// <summary>
    /// Annotate the operation filter field with a directive.
    /// </summary>
    /// <param name="name">
    /// The name of the directive.
    /// </param>
    /// <param name="arguments">
    /// The argument values of the directive.
    /// </param>
    ISortObjectOperationDescriptor Directive(string name, params ArgumentNode[] arguments);
}
