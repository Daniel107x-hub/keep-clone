"use client"

import useAuth from "@/app/hooks/useAuth";
import {redirect} from "next/navigation";
import {FormEvent, useActionState, useCallback, useEffect, useState} from "react";
import {deleteNote, getNotes} from "@/app/services/notesService";
import {NoteType} from "@/app/types/NoteType";
import TextareaAutosize from 'react-textarea-autosize';
import {addNote} from "@/app/actions/notes";
import {toast, ToastContainer} from "react-toastify";

type NoteProps = {
    id: number;
    title: string;
    content: string;
    onDelete: (id: number | undefined) => void;
}

type NoteFormProps = {
    onSubmit: (note: NoteType) => void;
}

function AddNoteForm(props: NoteFormProps): React.ReactNode{
    const [state, action, pending] = useActionState(addNote, undefined);
    const [title, setTitle] = useState<string>("");
    const [content, setContent] = useState<string>("");
    const { onSubmit } = props;

    useEffect(() => {
        if(pending) return;
        if(state?.message && state.data) {
            onSubmit(state.data);
        }
        if(state?.errors?.request) {
            toast.error(state.errors.request);
        }
        setContent("");
        setTitle("");
    }, [state, pending, onSubmit]);

    const handleSubmit = (e: FormEvent) => {
        if(title.trim() === "" && content.trim() === ""){
            e.preventDefault();
            return;
        }
    }
    return <form
        className="bg-neutral-700 fixed bottom-16 right-16 flex flex-col w-64 p-4 rounded-xl justify-between gap-4 text-neutral-200"
        action={action}
    >
        <input
            id="title"
            name="title"
            className="bg-transparent active:outline-0 focus:outline-0"
            placeholder="Title"
            value={title}
            onChange={e => setTitle(e.target.value)}
        />
        <TextareaAutosize
            id="content"
            name="content"
            className="bg-transparent resize-none active:outline-0 focus:outline-0 max-h-96"
            placeholder="Content"
            value={content}
            onChange={e => setContent(e.target.value)}
        />
        <button
            className="bg-neutral-800 p-2 rounded-xl enabled:hover:bg-neutral-600 disabled:cursor-not-allowed disabled:opacity-50"
            onClick={handleSubmit}
            disabled={pending}
        >Add</button>
    </form>
}

function Note(props: NoteProps): React.ReactNode{
    const {id, title, content, onDelete} = props;
    function handleDeleteNote(id: number){
        deleteNote(id)
            .then(() => {
                onDelete(id);
            })
            .catch((e) => {
                if(e?.response.status === 401) redirect("/Login");
                onDelete(undefined);
            });
    }

    return <div className="group border border-solid border-neutral-600 rounded-xl flex flex-col gap-1 p-4 relative">
        <button className="bg-neutral-700 hover:bg-neutral-600 w-6 h-6 absolute -right-2 -top-2 rounded-full opacity-0 group-hover:opacity-100 transition-opacity hover:text-red-300" onClick={()=> handleDeleteNote(id)}>x</button>
        { title && <h1 className="text-xl font-bold">{title}</h1> }
        { content && <p className="text-sm">{content}</p> }
    </div>
}

export default function Home() {
    const [isAuthenticated, isLoading] = useAuth();
    const [notes, setNotes] = useState<NoteType[]>([]);
    const [loadingNotes, setLoadingNotes] = useState<boolean>(true);
    const [isAddingNote, setIsAddingNote] = useState<boolean>(false);

    const handleNewNote = useCallback((note: NoteType) => {
        setNotes(prev => [...prev, note]);
        setIsAddingNote(false);
    }, []);
    const handleDeleteNote = (id: number | undefined) => {
        if(id === undefined) {
            toast.error("Unexpected error");
            return;
        }
        setNotes(notes => notes.filter((note: NoteType) => note.id !== id));
    }

    useEffect(() => {
        if(!isAuthenticated || isLoading) return;
        getNotes()
            .then(res => {
                const notes : NoteType[] = res.data;
                setNotes(notes);
            })
            .catch(e => {
                if(e?.response.status === 401) redirect("/Login");
            })
            .finally(() => {
                setLoadingNotes(false);
            });
    }, [isAuthenticated, isLoading]);

    if(isLoading) return <div>Loading...</div>
    if(!isAuthenticated) redirect("/Login");
    if(loadingNotes) return <div>Loading notes...</div>

    return (
    <div className="w-full p-4 grid grid-cols-3 gap-2 text-neutral-200">
    {
        notes.map((note: NoteType) => <Note key={note.id} {...note} onDelete={handleDeleteNote}/>)
    }
    { isAddingNote && <AddNoteForm onSubmit={handleNewNote}/> }
    <button className="bg-neutral-700 w-12 h-12 text-xl rounded-full fixed bottom-4 right-4 hover:bg-neutral-600 hover:text-amber-400" onClick={() => setIsAddingNote(prev => !prev)}>+</button>
        <ToastContainer/>
    </div>
    );
}