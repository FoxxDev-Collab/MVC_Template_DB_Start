using HLE.FamilyFinance.Data;
using HLE.FamilyFinance.Models.Entities;
using HLE.FamilyFinance.Models.Enums;
using HLE.FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HLE.FamilyFinance.Services;

public class TaxService(ApplicationDbContext context, ILogger<TaxService> logger) : ITaxService
{
    public async Task<List<TaxYearSummaryDto>> GetTaxYearsAsync(int householdId, CancellationToken ct = default)
    {
        return await context.TaxYears
            .AsNoTracking()
            .Include(t => t.Documents)
            .Where(t => t.HouseholdId == householdId)
            .OrderByDescending(t => t.Year)
            .Select(t => new TaxYearSummaryDto(
                t.Id,
                t.Year,
                t.FederalFilingStatus,
                t.State,
                t.IsFederalFiled,
                t.IsStateFiled,
                t.FederalRefund,
                t.StateRefund,
                t.Documents.Count,
                t.Documents.Count(d => d.IsReceived)
            ))
            .ToListAsync(ct);
    }

    public async Task<TaxYearDetailDto?> GetTaxYearAsync(int id, int householdId, CancellationToken ct = default)
    {
        var taxYear = await context.TaxYears
            .AsNoTracking()
            .Include(t => t.Documents)
            .Where(t => t.Id == id && t.HouseholdId == householdId)
            .FirstOrDefaultAsync(ct);

        if (taxYear == null) return null;

        return MapToDetailDto(taxYear);
    }

    public async Task<TaxYearDetailDto?> GetTaxYearByYearAsync(int year, int householdId, CancellationToken ct = default)
    {
        var taxYear = await context.TaxYears
            .AsNoTracking()
            .Include(t => t.Documents)
            .Where(t => t.Year == year && t.HouseholdId == householdId)
            .FirstOrDefaultAsync(ct);

        if (taxYear == null) return null;

        return MapToDetailDto(taxYear);
    }

    private static TaxYearDetailDto MapToDetailDto(TaxYear taxYear)
    {
        var documents = taxYear.Documents.Select(d => new TaxDocumentDto(
            d.Id,
            d.DocumentType,
            d.Issuer,
            d.Description,
            d.GrossAmount,
            d.FederalWithheld,
            d.StateWithheld,
            d.SocialSecurityWithheld,
            d.MedicareWithheld,
            d.IsReceived,
            d.ReceivedDate,
            d.ExpectedDate,
            d.Notes
        )).ToList();

        return new TaxYearDetailDto(
            taxYear.Id,
            taxYear.Year,
            taxYear.FederalFilingStatus,
            taxYear.State,
            taxYear.IsFederalFiled,
            taxYear.FederalFiledDate,
            taxYear.IsStateFiled,
            taxYear.StateFiledDate,
            taxYear.FederalRefund,
            taxYear.StateRefund,
            taxYear.RefundReceived,
            taxYear.RefundReceivedDate,
            taxYear.Notes,
            taxYear.Documents.Sum(d => d.GrossAmount ?? 0),
            taxYear.Documents.Sum(d => d.FederalWithheld ?? 0),
            taxYear.Documents.Sum(d => d.StateWithheld ?? 0),
            documents
        );
    }

    public async Task<TaxYear> CreateTaxYearAsync(int householdId, TaxYearCreateDto dto, CancellationToken ct = default)
    {
        // Check for duplicate year
        var exists = await context.TaxYears
            .AnyAsync(t => t.HouseholdId == householdId && t.Year == dto.Year, ct);

        if (exists)
            throw new InvalidOperationException($"Tax year {dto.Year} already exists");

        var taxYear = new TaxYear
        {
            HouseholdId = householdId,
            Year = dto.Year,
            FederalFilingStatus = dto.FederalFilingStatus,
            State = dto.State,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        context.TaxYears.Add(taxYear);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created tax year {Year} for household {HouseholdId}", dto.Year, householdId);
        return taxYear;
    }

    public async Task UpdateTaxYearAsync(int id, int householdId, TaxYearCreateDto dto, CancellationToken ct = default)
    {
        var taxYear = await context.TaxYears
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId, ct)
            ?? throw new InvalidOperationException("Tax year not found");

        taxYear.FederalFilingStatus = dto.FederalFilingStatus;
        taxYear.State = dto.State;
        taxYear.Notes = dto.Notes;
        taxYear.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteTaxYearAsync(int id, int householdId, CancellationToken ct = default)
    {
        var taxYear = await context.TaxYears
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId, ct)
            ?? throw new InvalidOperationException("Tax year not found");

        context.TaxYears.Remove(taxYear);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Deleted tax year {Year}", taxYear.Year);
    }

    public async Task MarkFederalFiledAsync(int id, int householdId, DateOnly filedDate, decimal? refundAmount, CancellationToken ct = default)
    {
        var taxYear = await context.TaxYears
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId, ct)
            ?? throw new InvalidOperationException("Tax year not found");

        taxYear.IsFederalFiled = true;
        taxYear.FederalFiledDate = filedDate;
        taxYear.FederalRefund = refundAmount;
        taxYear.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Marked federal taxes filed for year {Year}", taxYear.Year);
    }

    public async Task MarkStateFiledAsync(int id, int householdId, DateOnly filedDate, decimal? refundAmount, CancellationToken ct = default)
    {
        var taxYear = await context.TaxYears
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId, ct)
            ?? throw new InvalidOperationException("Tax year not found");

        taxYear.IsStateFiled = true;
        taxYear.StateFiledDate = filedDate;
        taxYear.StateRefund = refundAmount;
        taxYear.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Marked state taxes filed for year {Year}", taxYear.Year);
    }

    public async Task MarkRefundReceivedAsync(int id, int householdId, DateOnly receivedDate, CancellationToken ct = default)
    {
        var taxYear = await context.TaxYears
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId, ct)
            ?? throw new InvalidOperationException("Tax year not found");

        taxYear.RefundReceived = true;
        taxYear.RefundReceivedDate = receivedDate;
        taxYear.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task<TaxDocument> AddDocumentAsync(int taxYearId, int householdId, TaxDocumentCreateDto dto, CancellationToken ct = default)
    {
        var taxYear = await context.TaxYears
            .FirstOrDefaultAsync(t => t.Id == taxYearId && t.HouseholdId == householdId, ct)
            ?? throw new InvalidOperationException("Tax year not found");

        var document = new TaxDocument
        {
            TaxYearId = taxYearId,
            DocumentType = dto.DocumentType,
            Issuer = dto.Issuer,
            Description = dto.Description,
            GrossAmount = dto.GrossAmount,
            FederalWithheld = dto.FederalWithheld,
            StateWithheld = dto.StateWithheld,
            SocialSecurityWithheld = dto.SocialSecurityWithheld,
            MedicareWithheld = dto.MedicareWithheld,
            ExpectedDate = dto.ExpectedDate,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        context.TaxDocuments.Add(document);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Added {DocumentType} from {Issuer} to tax year {Year}", dto.DocumentType, dto.Issuer, taxYear.Year);
        return document;
    }

    public async Task UpdateDocumentAsync(int documentId, int householdId, TaxDocumentCreateDto dto, CancellationToken ct = default)
    {
        var document = await context.TaxDocuments
            .Include(d => d.TaxYear)
            .FirstOrDefaultAsync(d => d.Id == documentId && d.TaxYear.HouseholdId == householdId, ct)
            ?? throw new InvalidOperationException("Tax document not found");

        document.DocumentType = dto.DocumentType;
        document.Issuer = dto.Issuer;
        document.Description = dto.Description;
        document.GrossAmount = dto.GrossAmount;
        document.FederalWithheld = dto.FederalWithheld;
        document.StateWithheld = dto.StateWithheld;
        document.SocialSecurityWithheld = dto.SocialSecurityWithheld;
        document.MedicareWithheld = dto.MedicareWithheld;
        document.ExpectedDate = dto.ExpectedDate;
        document.Notes = dto.Notes;
        document.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task MarkDocumentReceivedAsync(int documentId, int householdId, DateOnly receivedDate, CancellationToken ct = default)
    {
        var document = await context.TaxDocuments
            .Include(d => d.TaxYear)
            .FirstOrDefaultAsync(d => d.Id == documentId && d.TaxYear.HouseholdId == householdId, ct)
            ?? throw new InvalidOperationException("Tax document not found");

        document.IsReceived = true;
        document.ReceivedDate = receivedDate;
        document.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteDocumentAsync(int documentId, int householdId, CancellationToken ct = default)
    {
        var document = await context.TaxDocuments
            .Include(d => d.TaxYear)
            .FirstOrDefaultAsync(d => d.Id == documentId && d.TaxYear.HouseholdId == householdId, ct)
            ?? throw new InvalidOperationException("Tax document not found");

        context.TaxDocuments.Remove(document);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<TaxDocumentDto>> GetPendingDocumentsAsync(int taxYearId, int householdId, CancellationToken ct = default)
    {
        var taxYear = await context.TaxYears
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taxYearId && t.HouseholdId == householdId, ct);

        if (taxYear == null) return [];

        return await context.TaxDocuments
            .AsNoTracking()
            .Where(d => d.TaxYearId == taxYearId && !d.IsReceived)
            .OrderBy(d => d.ExpectedDate)
            .Select(d => new TaxDocumentDto(
                d.Id,
                d.DocumentType,
                d.Issuer,
                d.Description,
                d.GrossAmount,
                d.FederalWithheld,
                d.StateWithheld,
                d.SocialSecurityWithheld,
                d.MedicareWithheld,
                d.IsReceived,
                d.ReceivedDate,
                d.ExpectedDate,
                d.Notes
            ))
            .ToListAsync(ct);
    }
}
