public abstract class BaseConfig {
    protected string FilePath { get; set; }

    public BaseConfig(string filePath) {
        FilePath = filePath;
    }

    public virtual void Load() {
        // Default implementation or throw an exception
        throw new NotImplementedException("Load method must be implemented.");
    }

    public virtual void Save() {
        // Default implementation or throw an exception
        throw new NotImplementedException("Save method must be implemented.");
    }

    public virtual bool Validate() {
        // Default implementation or throw an exception
        throw new NotImplementedException("Validate method must be implemented.");
    }
}
