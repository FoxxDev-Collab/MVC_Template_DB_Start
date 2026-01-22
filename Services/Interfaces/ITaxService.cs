using HLE.FamilyFinance.Models.Entities;
using HLE.FamilyFinance.Models.Enums;

namespace HLE.FamilyFinance.Services.Interfaces;

public record TaxYearSummaryDto(
    int Id,
    int Year,
    TaxFilingStatus FederalFilingStatus,
    string? State,
    bool IsFederalFiled,
    bool IsStateFiled,
    decimal? FederalRefund,
    decimal? StateRefund,
    int DocumentCount,
    int ReceivedDocumentCount
);

public record TaxYearDetailDto(
    int Id,
    int Year,
    TaxFilingStatus FederalFilingStatus,
    string? State,
    bool IsFederalFiled,
    DateOnly? FederalFiledDate,
    bool IsStateFiled,
    DateOnly? StateFiledDate,
    decimal? FederalRefund,
    decimal? StateRefund,
    bool RefundReceived,
    DateOnly? RefundReceivedDate,
    string? Notes,
    decimal TotalGrossIncome,
    decimal TotalFederalWithheld,
    decimal TotalStateWithheld,
    List<TaxDocumentDto> Documents
);

public record TaxDocumentDto(
    int Id,
    TaxDocumentType DocumentType,
    string Issuer,
    string? Description,
    decimal? GrossAmount,
    decimal? FederalWithheld,
    decimal? StateWithheld,
    decimal? SocialSecurityWithheld,
    decimal? MedicareWithheld,
    bool IsReceived,
    DateOnly? ReceivedDate,
    DateOnly? ExpectedDate,
    string? Notes
);

public record TaxYearCreateDto(
    int Year,
    TaxFilingStatus FederalFilingStatus,
    string? State,
    string? Notes
);

public record TaxDocumentCreateDto(
    TaxDocumentType DocumentType,
    string Issuer,
    string? Description,
    decimal? GrossAmount,
    decimal? FederalWithheld,
    decimal? StateWithheld,
    decimal? SocialSecurityWithheld,
    decimal? MedicareWithheld,
    DateOnly? ExpectedDate,
    string? Notes
);

public interface ITaxService
{
    Task<List<TaxYearSummaryDto>> GetTaxYearsAsync(int householdId, CancellationToken ct = default);
    Task<TaxYearDetailDto?> GetTaxYearAsync(int id, int householdId, CancellationToken ct = default);
    Task<TaxYearDetailDto?> GetTaxYearByYearAsync(int year, int householdId, CancellationToken ct = default);
    Task<TaxYear> CreateTaxYearAsync(int householdId, TaxYearCreateDto dto, CancellationToken ct = default);
    Task UpdateTaxYearAsync(int id, int householdId, TaxYearCreateDto dto, CancellationToken ct = default);
    Task DeleteTaxYearAsync(int id, int householdId, CancellationToken ct = default);
    Task MarkFederalFiledAsync(int id, int householdId, DateOnly filedDate, decimal? refundAmount, CancellationToken ct = default);
    Task MarkStateFiledAsync(int id, int householdId, DateOnly filedDate, decimal? refundAmount, CancellationToken ct = default);
    Task MarkRefundReceivedAsync(int id, int householdId, DateOnly receivedDate, CancellationToken ct = default);
    Task<TaxDocument> AddDocumentAsync(int taxYearId, int householdId, TaxDocumentCreateDto dto, CancellationToken ct = default);
    Task UpdateDocumentAsync(int documentId, int householdId, TaxDocumentCreateDto dto, CancellationToken ct = default);
    Task MarkDocumentReceivedAsync(int documentId, int householdId, DateOnly receivedDate, CancellationToken ct = default);
    Task DeleteDocumentAsync(int documentId, int householdId, CancellationToken ct = default);
    Task<List<TaxDocumentDto>> GetPendingDocumentsAsync(int taxYearId, int householdId, CancellationToken ct = default);
}
