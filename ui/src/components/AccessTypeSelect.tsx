import { ACCESS_TYPES } from '../constants/accessTypes';

interface AccessTypeSelectProps {
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
  id?: string;
}

export const AccessTypeSelect = ({ value, onChange, required = true, id = 'accessType' }: AccessTypeSelectProps) => {
  return (
    <div className="form-group">
      <label htmlFor={id}>Access Type {required && '*'}</label>
      <select
        id={id}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        required={required}
      >
        {ACCESS_TYPES.map((type) => (
          <option key={type.value} value={type.value}>
            {type.label} - {type.description}
          </option>
        ))}
      </select>
    </div>
  );
};
