using System.Globalization;

using LibraryDesk.Legacy;
using Xunit;

namespace LibraryDesk.Legacy.Tests;

public sealed class LegacyCharacterizationTests
{
    public LegacyCharacterizationTests()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory]
    [InlineData(null, LoanState.New, "a@b.c", "ERR: null")]
    [InlineData("empty", LoanState.New, "a@b.c", "ERR: empty")]
    [InlineData("two", LoanState.Draft, "a@b.c", "ERR: state")]
    [InlineData("two", LoanState.New, "no-mail", "ERR: mail")]
    public void Handle_BadInput_ReturnsErrorCode(
        string? kindOfItems,
        LoanState state,
        string mail,
        string expected)
    {
        var items = kindOfItems switch
        {
            "two" => TwoLines(),
            "empty" => new List<LoanLine>(),
            _ => null,
        };
        var sut = new LegacyLoanProcessor();

        var request = Request(mail, ReaderKind.Regular, 0, items, state, false);

        var actual = sut.Handle(request);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Handle_VipLoan_ReturnsExactReport()
    {
        var sut = new LegacyLoanProcessor();

        var request = Request(
            "a@b.c",
            ReaderKind.Vip,
            3,
            TwoLines(),
            LoanState.New,
            false);

        var report = sut.Handle(request);
        var expected =
            "Документ #1001\n"
            + "Клієнт: Іван\n"
            + "A-1 x2 = 300.00 UAH\n"
            + "B-2 x1 = 400.00 UAH\n"
            + "Знижка: 105.00 UAH\n"
            + "Доставка: 60.00 UAH\n"
            + "Разом: 655.00 UAH\n";

        Assert.Equal(expected, report);
    }

    [Theory]
    [InlineData(ReaderKind.Regular, 0, 760)]
    [InlineData(ReaderKind.Vip, 0, 655)]
    [InlineData(ReaderKind.Staff, 0, 550)]
    [InlineData(ReaderKind.Regular, 11, 725)]
    public void Preview_ByReaderKind_ReturnsTotal(
        ReaderKind kind,
        int done,
        decimal expected)
    {
        var sut = new LegacyLoanProcessor();
        var reader = new Reader { Kind = kind, DoneCount = done };

        var total = sut.Preview(reader, TwoLines());

        Assert.Equal(expected, total);
    }

    [Fact]
    public void Preview_ExactlyAtFreeShippingBoundary_DoesNotAddShipping()
    {
        var items = new List<LoanLine>
        {
            new() { Code = "BOUNDARY", Qty = 1, Price = 1000m },
        };
        var sut = new LegacyLoanProcessor();
        var reader = new Reader { Kind = ReaderKind.Regular };

        var total = sut.Preview(reader, items);

        Assert.Equal(1000m, total);
    }

    [Fact]
    public void DescribeReader_Vip_BuildsCaption()
    {
        var reader = new Reader
        {
            Name = " іван ",
            Email = "IVAN@MAIL.COM",
            Kind = ReaderKind.Vip,
            DoneCount = 12,
            SinceUtc = new DateTime(2020, 1, 1),
        };
        var years = DateTime.Now.Year - 2020;
        var expected = "ІВАН [VIP] [ЛОЯЛЬНИЙ] <ivan@mail.com> стаж " + years;

        var actual = reader.Describe();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Handle_WithMailRequested_PreservesExactLog()
    {
        var sut = new LegacyLoanProcessor();

        var request = Request(
            "a@b.c",
            ReaderKind.Regular,
            0,
            TwoLines(),
            LoanState.Paid,
            true);

        sut.Handle(request);

        Assert.Equal("ok 1001\nmail -> a@b.c\n", sut.DumpLog());
    }

    private static List<LoanLine> TwoLines() =>
    [
        new() { Code = "A-1", Qty = 2, Price = 150m },
        new() { Code = "B-2", Qty = 1, Price = 400m },
    ];

    private static LoanRequest Request(
        string email,
        ReaderKind kind,
        int doneCount,
        IReadOnlyList<LoanLine>? items,
        LoanState state,
        bool sendMail)
    {
        var reader = new Reader
        {
            Id = 7,
            Name = "Іван",
            Email = email,
            Kind = kind,
            DoneCount = doneCount,
        };

        return new LoanRequest(1001, reader, items, state, "UAH", sendMail);
    }
}
