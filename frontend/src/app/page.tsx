"use client"

import useAuth from "@/app/hooks/useAuth";
import {redirect} from "next/navigation";
import {FormEvent, useActionState, useEffect, useState} from "react";
import {getNotes} from "@/app/services/notesService";
import {NoteType} from "@/app/types/NoteType";
import TextareaAutosize from 'react-textarea-autosize';
import {addNote} from "@/app/actions/notes";
import {toast} from "react-toastify";

type NoteProps = {
    title: string;
    content: string;
}

type NoteFormProps = {
    onSubmit: (title: string, content: string) => void;
}

function NewNote(props: NoteFormProps): React.ReactNode{
    const [state, action, pending] = useActionState(addNote, undefined);
    const [title, setTitle] = useState<string>("");
    const [content, setContent] = useState<string>("");
    const { onSubmit } = props;

    useEffect(() => {
        if(pending) return;
        if(state?.message) {
            toast.success(state.message);
        }
        if(state?.errors?.request) {
            toast.error(state.errors.request);
        }
    }, [state, pending]);

    const handleSubmit = (e: FormEvent) => {
        onSubmit(title, content);
        if(title.trim() === "" || content.trim() === ""){
            e.preventDefault();
            return;
        }
    }
    return <form
        className="bg-neutral-700 absolute bottom-16 right-16 flex flex-col w-64 p-4 rounded-xl justify-between gap-4 text-neutral-200"
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
            className="bg-neutral-800 p-2 rounded-xl hover:bg-neutral-600"
            onClick={handleSubmit}
            disabled={pending}
        >Add</button>
    </form>
}

function Note(props: NoteProps): React.ReactNode{
    const {title, content} = props;
    return <div className="group border border-solid border-neutral-600 rounded-xl flex flex-col gap-1 p-4 relative">
        <button className="bg-neutral-700 hover:bg-neutral-600 w-6 h-6 absolute -right-2 -top-2 rounded-full opacity-0 group-hover:opacity-100 transition-opacity">x</button>
        { title && <h1 className="text-xl">{title}</h1> }
        { content && <p className="text-sm">{content}</p> }
    </div>
}

export default function Home() {
    const [isAuthenticated, isLoading] = useAuth();
    const [notes, setNotes] = useState<NoteType[]>([]);
    const [loadingNotes, setLoadingNotes] = useState<boolean>(true);
    const [isAddingNote, setIsAddingNote] = useState<boolean>(false);

    useEffect(() => {
    if(!isAuthenticated || isLoading) return;
    getNotes()
    .then(res => {
        const notes : NoteType[] = res.data;
        setNotes(notes);
    })
    .catch(err => {
        console.log(err);
    })
    .finally(() => {
        setLoadingNotes(false);
    });
    }, [isAuthenticated, isLoading]);

    if(isLoading) return <div>Loading...</div>
    if(!isAuthenticated) redirect("/Login");
    if(loadingNotes) return <div>Loading notes...</div>
    const handleNewNote = (title: string, content: string) => {
        setNotes(prev => [...prev, {title, content}]);
        setIsAddingNote(false);
    }
    return (
    <div className="w-full p-4 grid grid-cols-3 gap-2 text-neutral-200">
    {
        notes.map((note: NoteType, index: number) => <Note key={index} {...note}/>)
    }
    { isAddingNote && <NewNote onSubmit={handleNewNote}/> }
    <button className="bg-neutral-700 w-12 h-12 text-xl rounded-full absolute bottom-4 right-4 hover:bg-neutral-600 hover:text-amber-400" onClick={() => setIsAddingNote(prev => !prev)}>+</button>
    </div>
    );
}