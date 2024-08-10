import { ByLawListView } from 'mods/ByLawListView/ByLawListView';
import styles from './SidePanel.module.scss';
import { useState } from 'react';
import { SidePanelHeader } from 'mods/SidePanelHeader/SidePanelHeader';
import { SidePanelViews } from 'mods/types';
import { ByLawEditorView } from 'mods/ByLawEditorView/ByLawEditorView';

export const SidePanel = () => {
    let [currentView, setCurrentView] = useState<SidePanelViews>('bylaws');
    let [searchQuery, setSearchQuery] = useState<string | undefined>();
    let onSearchChange = (text: string) => {
        setSearchQuery(text);
    }
    let onViewChange = (newView: SidePanelViews) => {
        console.log("New View");
        setCurrentView(newView);
    }

    return (        
        <div className={styles.view}>
            <SidePanelHeader currentView={currentView} onSearchQueryChange={onSearchChange} onViewChange={onViewChange} />
            {currentView == 'bylaws'? <ByLawListView searchQuery={searchQuery} /> : <ByLawEditorView />}
        </div>
    )
}