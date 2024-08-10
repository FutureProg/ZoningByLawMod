import { ByLawListView } from 'mods/ByLawListView/ByLawListView';
import styles from './SidePanel.module.scss';
import { useState } from 'react';

export const SidePanel = () => {
    let [searchQuery, setSearchQuery] = useState('');

    return (        
        <div className={styles.view}>
            <ByLawListView searchQuery={searchQuery} />
        </div>
    )
}