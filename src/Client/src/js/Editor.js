import React from 'react';
import MonacoEditor from 'react-monaco-editor';
import PropTypes from 'prop-types';
import ReactResizeDetector from 'react-resize-detector';


class Editor extends React.Component {
    editor = null;

    constructor(props) {
        super(props);
    }

    editorDidMount = (editor, monaco) => {
        this.props.editorDidMount();
        this.editor = editor;
    };

    onChange = (newValue, e) => {
        this.props.onChange(newValue);
    };

    onResize = () => {
        if (this.editor !== null)
            this.editor.layout();
    };

    render() {
        const options = {
            selectOnLineNumbers: true,
            lineNumbers: true,
            theme: 'vs-light',
            readOnly: this.props.isReadOnly,
            renderWhitespace: "all",
            minimap: {
                enabled: false
            }
        };
        return (
            <div style={{height: '50vh', overflow: 'hidden'}}>
                <ReactResizeDetector handleWidth handleHeight onResize={this.onResize}/>
                <MonacoEditor
                    language={this.props.language}
                    value={this.props.value}
                    options={options}
                    onChange={this.onChange}
                    editorDidMount={this.editorDidMount}
                />
            </div>
        );
    }
}

function noop() {
}

Editor.propTypes = {
    onChange: PropTypes.func,
    value: PropTypes.string,
    language: PropTypes.string,
    isReadOnly: PropTypes.bool,
    editorDidMount: PropTypes.func
};

Editor.defaultProps = {
    onChange: noop,
    value: "",
    language: "fsharp",
    isReadOnly: false,
    editorDidMount: noop
};

export default Editor;
