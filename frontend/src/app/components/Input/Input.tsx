type InputProps = {
    label: string,
    id: string,
    placeholder: string,
    type: string,
    defaultValue?: string | number | undefined
};

const Input = ({ label, id, placeholder, type, defaultValue}: InputProps) => {
    return <div className="flex flex-col w-full py-2 gap-1">
        <label htmlFor={id} className="text-xs">{label}</label>
        <input
            id={id}
            name={id}
            placeholder={placeholder}
            type={type}
            className="w-full bg-slate-600 rounded text-slate-100 px-1 focus:outline-0"
            defaultValue={defaultValue}
        />
    </div>
}

export default Input