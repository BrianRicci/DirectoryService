using Shared;

namespace DirectoryService.Application.Locations.Fails;

public partial class Errors
{
    public static class Locations
    {
        public static Error ValueIsInvalid() =>
            Error.Validation("value.is.invalid", "Неправильное значение");
    }
}