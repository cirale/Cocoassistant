using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cocoassistant {
    public class apps {
        private string _key;
        private string _path;

        public string key{
            get { return this._key; }
            set { this._key = value; }
        }
        public string path {
            get { return this._path; }
            set { this._path = value; }
        }
    }
}
