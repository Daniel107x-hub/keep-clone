type InputProps = {
    label: string,
    id: string,
    placeholder: string,
    type: string
};

const Input = ({ label, id, placeholder, type}: InputProps) => {
    return <div className="flex flex-col w-full py-2 gap-1">
        <label htmlFor={id} className="text-xs">{label}</label>
        <input id={id} name={id} placeholder={placeholder} type={type} className="w-full bg-slate-600 rounded text-slate-100 px-1"/>
    </div>
}

export default Input