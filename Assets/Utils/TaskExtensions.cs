﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Utils {
  public static class TaskExtensions {
    public static IEnumerator AsIEnumerator(this Task task) {
      while (!task.IsCompleted) {
        yield return null;
      }

      if (task.IsFaulted) {
        ExceptionDispatchInfo.Capture(task.Exception).Throw();
      }
    }

    public static IEnumerator<T> AsIEnumerator<T>(this Task<T> task)
      where T : class {
      while (!task.IsCompleted) {
        yield return null;
      }

      if (task.IsFaulted) {
        ExceptionDispatchInfo.Capture(task.Exception).Throw();
      }

      yield return task.Result;
    }
  }
}
