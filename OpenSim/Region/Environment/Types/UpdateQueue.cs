/*
* Copyright (c) Contributors, http://opensimulator.org/
* See CONTRIBUTORS.TXT for a full list of copyright holders.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the OpenSim Project nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
* 
*/

using System.Collections.Generic;
using libsecondlife;
using OpenSim.Region.Environment.Scenes;

namespace OpenSim.Region.Environment.Types
{
    public class UpdateQueue
    {
        private Queue<SceneObjectPart> m_queue;

        private List<LLUUID> m_ids;

        public int Count
        {
            get { return m_queue.Count; }
        }

        public UpdateQueue()
        {
            m_queue = new Queue<SceneObjectPart>();
            m_ids = new List<LLUUID>();
        }
        public void Clear()
        {
            lock (m_ids)
            {
                m_ids.Clear();
            }
            lock (m_queue)
            {
                m_queue.Clear();
            }
        }

        public void Enqueue(SceneObjectPart part)
        {
            lock (m_ids)
            {
                if (!m_ids.Contains(part.UUID))
                {
                    m_ids.Add(part.UUID);
                    m_queue.Enqueue(part);
                }
            }
        }

        public SceneObjectPart Dequeue()
        {
            SceneObjectPart part = null;
            if (m_queue.Count > 0)
            {
                part = m_queue.Dequeue();
                lock (m_ids)
                {
                    m_ids.Remove(part.UUID);
                }
            }

            return part;
        }
    }
}