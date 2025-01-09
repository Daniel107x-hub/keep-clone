import { NewNoteFormSate } from "@/app/lib/definitions";
import api from "@/app/configs/axiosConfig";
import {NoteType} from "@/app/types/NoteType";
import {redirect} from "next/navigation";

export async function addNote(state: NewNoteFormSate, formData: FormData): Promise<NewNoteFormSate>{
    const payload = {
        title: formData.get('title'),
        content: formData.get('content')
    }
    try{
        const response = await api.post('/Notes', payload);
        const note: NoteType = response.data;
        return {
            message: 'Note created successfully',
            data: note
        }
        // @ts-expect-error Exception
    }catch (e: never) {
        if(e?.response.status === 401) redirect("/Login");
        return {errors: {request: e?.response.data}}
    }
}