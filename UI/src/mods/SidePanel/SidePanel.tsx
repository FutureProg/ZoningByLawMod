import { ByLawListView } from 'mods/ByLawListView/ByLawListView';
import styles from './SidePanel.module.scss';
import { useState } from 'react';
import { SidePanelHeader } from 'mods/SidePanelHeader/SidePanelHeader';
import { SidePanelViews } from 'mods/types';

export const SidePanel = () => {
    let [currentView, setCurrentView] = useState<SidePanelViews>('bylaws');
    let [searchQuery, setSearchQuery] = useState('');
    let onSearchChange = (text: string) => {
        setSearchQuery(text);
    }
    let onViewChange = (newView: SidePanelViews) => {
        setCurrentView(newView);
    }

    return (        
        <div className={styles.view}>
            <SidePanelHeader currentView={currentView} onSearchQueryChange={onSearchChange} onViewChange={onViewChange} />
            <ByLawListView searchQuery={searchQuery} />
        </div>
    )
}