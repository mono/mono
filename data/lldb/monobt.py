import lldb

def print_frames(thread, num_frames, current_thread):
    # TODO: Make output header similar to bt.
    print('%c thread #%i' % ('*' if current_thread else ' ', thread.idx))

    if current_thread:
        selected_frame = thread.GetSelectedFrame()

    for frame in thread.frames[:+num_frames]:
        pc = str(frame.addr)
        var = frame
        function_name = frame.GetFunctionName()
        if function_name == "interp_exec_method_full":
            try:
                s = 'frame->imethod->method'
                methoddesc = frame.EvaluateExpression('(char*) mono_method_full_name (' + s + ', 1)').summary[1:-1]

                ipoffset = frame.EvaluateExpression('ip').GetValueAsUnsigned()
                insn = ''
                if ipoffset != 0:
                    ipoffset -= frame.EvaluateExpression('imethod->code').GetValueAsUnsigned()
                    insn = "\"" + frame.EvaluateExpression('mono_interp_opname [*ip]').summary[1:-1] + "\""
                var = '%s @ %d %s || %s' % (methoddesc, ipoffset, insn, frame)
            except Exception as e:
                print("DBG: execfail:" + str(e))
        elif function_name == "mono_interp_transform_method":
            try:
                s = 'runtime_method->method'
                klassname = frame.EvaluateExpression('(char*) ' + s + '->klass->name').summary[1:-1]
                methodname = frame.EvaluateExpression('(char*) ' + s + '->name').summary[1:-1]
                var = 'transforming %s::%s || %s' % (klassname, methodname, frame)
            except Exception as e:
                print("DBG: transformfail:" + str(e))
        elif pc[0] == '0':
            try:
                framestr = frame.EvaluateExpression('(char*)mono_pmip((void*)%s)' % pc).summary[1:-1]
                var = 'frame #%i: %s%s' % (frame.idx, pc, framestr)
            except:
                pass

        print('  %c %s' % ('*' if current_thread and frame.idx == selected_frame.idx else ' ', var))

def monobt(debugger, command, result, dict):
    opts = {'all_bt': False, 'num_frames': None}

    if command == 'all':
        opts['all_bt'] = True
    elif command.isdigit():
        opts['num_frames'] = int(command)
    elif command != '':
        print('error: monobt [<number>|all]')
        return

    target = debugger.GetSelectedTarget()
    process = target.process

    if not process.IsValid():
        print('error: invalid process')
        return

    if opts['all_bt']:
        for thread in process.threads:
            print_frames(thread, len(thread), process.selected_thread == thread)
            print('')
    else:
        thread = process.selected_thread
        num_frames = len(thread) if opts['num_frames'] is None else opts['num_frames']
        print_frames(thread, num_frames, True)

    return None

def __lldb_init_module (debugger, dict):
    # This initializer is being run from LLDB in the embedded command interpreter
    # Add any commands contained in this module to LLDB
    debugger.HandleCommand('command script add -f monobt.monobt monobt')
    debugger.HandleCommand('command alias mbt monobt')
    print('"monobt" command installed')
