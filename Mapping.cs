class Mapping{
	public Mapping(string line){
		var parts = line.Split('\t');
		if (parts.Length != 3){
			throw new Exception("Неправильная конфигурация поля " + line);
		}
		JsonField = parts[0];
		TableColumn = parts[1];
		ColumnType = parts[2];
	}
	public readonly string JsonField;
	public readonly string TableColumn;
	public readonly string ColumnType;
}