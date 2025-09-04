import { useLocalization } from "cs2/l10n";

interface FieldDataBase {
    id: string;
    label: string;
    options?: { image?: string; label: string; value: any }[];
}
interface CheckboxFieldData extends FieldDataBase {
    fieldType: 'checkbox';
    value: any[]; // array of selected values
}
interface RadioFieldData extends FieldDataBase {
    fieldType: 'radio';
    value: any; // single selected value
}
interface SelectFieldData extends FieldDataBase {
    fieldType: 'select';
    value: any; // single selected value
}
interface TextFieldData extends FieldDataBase {
    fieldType: 'text';
    value: string;
    validationRegex?: string;
}
interface NumberFieldData extends FieldDataBase {
    fieldType: 'number';
    slider?: boolean;
    value: number;
    min?: number;
    max?: number;
    step?: number;
}
interface RangeFieldData extends FieldDataBase {
    fieldType: 'range';
    value: [number, number]; // [min, max]
    min?: number;
    max?: number;
    step?: number;
}
export type FieldData = CheckboxFieldData | RadioFieldData | SelectFieldData | TextFieldData | NumberFieldData | RangeFieldData;

export interface DynamicInputProps {
    fieldData: FieldData;
    onChange: (id: string, value: any) => void;
}

export const DynamicInput = ({ fieldData, onChange }: DynamicInputProps) => {
    const { id, fieldType, label, options, value } = fieldData;
    const { translate } = useLocalization();

    if (fieldType == 'checkbox') {
        return (
            <>
                {
                    options?.map((option, index) => (
                        <span key={index}>
                            <input type="checkbox" id={`${id}-${option.value}`} checked={value.includes(option.value)} onChange={(e) => {
                                const newValue = e.target.checked
                                    ? [...value, option.value]
                                    : value.filter((v) => v !== option.value);
                                onChange(id, newValue);
                            }} />
                            {option.image && <img src={option.image} alt={option.label} />}
                            <label htmlFor={`${id}-${option.value}`}>{option.label}</label>
                        </span>
                    ))
                }
            </>
        )
    } else if (fieldType == 'radio') {
        return (
            <>
                {
                    options?.map((option, index) => (
                        <span key={index}>
                            <input type="radio" id={`${id}-${option.value}`} name={id} checked={value === option.value} onChange={() => onChange(id, option.value)} />
                            {option.image && <img src={option.image} alt={option.label} />}
                            <label htmlFor={`${id}-${option.value}`}>{option.label}</label>
                        </span>
                    ))
                }
            </>
        )
    } else if (fieldType == 'select') {
        return (
            <select id={id} value={value} onChange={(e) => onChange(id, e.target.value)}>
                {options?.map((option, index) => (
                    <option key={index} value={option.value}>
                        {option.image && <img src={option.image} alt={option.label} />}
                        {option.label}
                    </option>
                ))}
            </select>
        )
    } else if (fieldType == 'text') {
        return (
            <>
                <input type="text" id={id} value={value} onChange={(e) => onChange(id, e.target.value)} />
                {fieldData.validationRegex && !new RegExp(fieldData.validationRegex).test(value) && (<div style={{ color: 'red' }}>{translate("ZBL.Validation[Invalid Input]", "Invalid Input")}</div>)}
            </>
        )
    } else if (fieldType == 'number') {
        if (fieldData.slider) {
            const { min = 0, max = undefined, step = 1 } = fieldData;
            return (
                <input type="range" id={id} value={value} min={min} max={max} step={step} onChange={(e) => onChange(id, Number(e.target.value))} />
            )
        }
        return (
            <input type="number" id={id} value={value} onChange={(e) => onChange(id, e.target.value)} />
        )
    } else if (fieldType == 'range') {
        const { min = 0, max = undefined, step = 1 } = fieldData;
        return (
            <>
                <input type="number" id={id} value={value[0]} min={min} max={max} step={step} onChange={(e) => onChange(id, [Number(e.target.value), value[1]])} />
                <input type="number" id={`${id}-range`} value={value[1]} min={min} max={max} step={step} onChange={(e) => onChange(id, [value[0], Number(e.target.value)])} />
            </>
        )
    }
}