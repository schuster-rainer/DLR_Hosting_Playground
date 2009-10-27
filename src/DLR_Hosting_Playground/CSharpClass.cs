namespace DLR_hosting
{
	public class CSharpClass
	{
		public CSharpClass(string message)
		{
			Var = message;
		}
		
		private string m_var;
		public string Var
		{
			get { return GetType() + " says: " + m_var; }
			set { m_var = value; }
		}
	}
}
