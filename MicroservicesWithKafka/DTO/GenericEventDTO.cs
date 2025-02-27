namespace MicroservicesWithKafka.DTO
{
    public class GenericEventDTO<T>
    {
        public string EventType { get; set; }
        public T Data { get; set; }
    }
}
