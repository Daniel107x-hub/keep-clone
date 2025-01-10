import styles from './Loader.module.css';

type LoaderProps = {
    fullscreen?: boolean
}

function Loader({fullscreen}: LoaderProps): React.ReactNode{
    const loader = <span className={styles.loader}/>;
    if(fullscreen) return <div className="fixed top-0 left-0 w-screen h-screen flex justify-center items-center z-10">
        {loader}
    </div>
    return loader;
}

export default Loader;