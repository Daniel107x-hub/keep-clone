import { NewNoteFormSate } from "@/app/lib/definitions";
import api from "@/app/configs/axiosConfig";

export async function addNote(state: NewNoteFormSate, formData: FormData): Promise<NewNoteFormSate>{
    const payload = {
        title: formData.get('title'),
        content: formData.get('content')
    }
    try{
        await api.post('/Note', payload);
        return {message: 'Note created successfully'}
        // @ts-expect-error Exception
    }catch (e: never) {
        return {errors: {request: e?.response.data}}
    }
}