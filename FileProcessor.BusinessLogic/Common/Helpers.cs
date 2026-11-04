namespace FileProcessor.BusinessLogic.Common
{
    using System;
    using Shared.Extensions;

    public static class Helpers
    {
        public static Guid CalculateFileImportLogAggregateId(DateTimeOffset importLogDateTime,
                                                          Guid estateId)
        {
            Guid aggregateId = GuidCalculator.Combine(estateId, importLogDateTime.DateTime.ToGuid());
            return aggregateId;
        }
    }
}