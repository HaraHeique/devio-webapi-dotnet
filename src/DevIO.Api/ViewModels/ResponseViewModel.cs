namespace DevIO.Api.ViewModels
{
    public class ResponseViewModel
    {
        public bool Success { get; set; }

        public object Data { get; set; }
        
        public string[] Errors { get; set; }
    }
}
