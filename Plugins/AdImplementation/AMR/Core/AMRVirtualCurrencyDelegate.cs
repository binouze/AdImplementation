namespace AMR
{
    public interface AMRVirtualCurrencyDelegate
    {
        void didSpendVirtualCurrency(string network, string currency, double amount);
    }
}