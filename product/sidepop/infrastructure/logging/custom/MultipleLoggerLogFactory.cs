namespace sidepop.infrastructure.logging.custom
{
    using System;

    public class MultipleLoggerLogFactory : LogFactory
    {
        public Logger create_logger_bound_to(Object type)
        {
            return new Log4NetLogger();
        }        
    }
}