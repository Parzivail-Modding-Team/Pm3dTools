namespace BlockbenchToPmodel.PM3D
{
    internal class ModelObjectInfo
    {
        public string ObjectName { get; }
        public string MaterialName { get; }

        public ModelObjectInfo(string objectName, string materialName)
        {
            ObjectName = objectName;
            MaterialName = materialName;
        }

        protected bool Equals(ModelObjectInfo other)
        {
            return string.Equals(ObjectName, other.ObjectName) && string.Equals(MaterialName, other.MaterialName);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ModelObjectInfo) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((ObjectName != null ? ObjectName.GetHashCode() : 0) * 397) ^ (MaterialName != null ? MaterialName.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ModelObjectInfo left, ModelObjectInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelObjectInfo left, ModelObjectInfo right)
        {
            return !Equals(left, right);
        }
    }
}