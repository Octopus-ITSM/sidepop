namespace sidepop.infrastructure.logging
{
    using custom;

    public static class Log
    {
        public static Logger bound_to(object object_that_needs_logging)
        {
            return new Log4NetLogger();
        }
    }
}