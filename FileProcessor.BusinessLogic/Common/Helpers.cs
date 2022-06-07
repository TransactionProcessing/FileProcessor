namespace FileProcessor.BusinessLogic.Common
{
    using System;
    using Shared.Extensions;

    public static class Helpers
    {
        public static Guid CalculateFileImportLogAggregateId(DateTime importLogDateTime,
                                                          Guid estateId)
        {
            Guid aggregateId = GuidCalculator.Combine(estateId, importLogDateTime.ToGuid());
            return aggregateId;
        }
    }
}